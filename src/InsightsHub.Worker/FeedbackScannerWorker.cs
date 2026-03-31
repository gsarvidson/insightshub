using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsightsHub.Api.Data;
using InsightsHub.Api.Data.Entities;
using InsightsHub.Api.Repositories.Feedback;
using Microsoft.EntityFrameworkCore;

namespace InsightsHub.Worker;

public class FeedbackScannerWorker(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<FeedbackScannerWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollSeconds = configuration.GetValue("Worker:PollIntervalSeconds", 30);
        logger.LogInformation("FeedbackScannerWorker started. Polling every {Interval}s.", pollSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled error in poll cycle.");
            }

            await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InsightsHubDbContext>();

        var batchSize = configuration.GetValue("Worker:BatchSize", 10);
        var parallelism = configuration.GetValue("Worker:Parallelism", 4);

        // Fetch IDs only — each parallel task loads the full entity with its own scope
        var pendingIds = await db.FeedbackItems
            .Where(f => !f.IsAiProcessed)
            .OrderBy(f => f.CreatedAt)
            .Take(batchSize)
            .Select(f => f.Id)
            .ToListAsync(ct);

        if (pendingIds.Count == 0) return;

        logger.LogInformation(
            "Processing {Count} pending feedback item(s) with parallelism {Parallelism}.",
            pendingIds.Count, parallelism);

        // Shared opportunity snapshot — lock guards concurrent reads + additions
        var opportunities = await db.Opportunities.ToListAsync(ct);
        var oppLock = new object();

        await Parallel.ForEachAsync(
            pendingIds,
            new ParallelOptions { MaxDegreeOfParallelism = parallelism, CancellationToken = ct },
            async (itemId, innerCt) =>
            {
                using var itemScope = scopeFactory.CreateScope();
                var itemDb = itemScope.ServiceProvider.GetRequiredService<InsightsHubDbContext>();
                var feedbackRepo = itemScope.ServiceProvider.GetRequiredService<IFeedbackRepository>();

                var item = await itemDb.FeedbackItems
                    .Include(f => f.Tags)
                    .Include(f => f.DataSources)
                    .FirstOrDefaultAsync(f => f.Id == itemId, innerCt);

                if (item is null) return;

                await ProcessItemAsync(item, opportunities, oppLock, itemDb, feedbackRepo, innerCt);
            });
    }

    private async Task ProcessItemAsync(
        FeedbackItemEntity item,
        List<OpportunityEntity> opportunities,
        object oppLock,
        InsightsHubDbContext db,
        IFeedbackRepository feedbackRepo,
        CancellationToken ct)
    {
        try
        {
            var result = await CallClaudeAsync(item, opportunities, ct);
            if (result is null)
            {
                logger.LogWarning("No AI result for feedback {Id} — will retry next poll.", item.Id);
                return;
            }

            // Override sentiment when it hasn't been set yet (null = newly imported or added)
            if (item.Sentiment is null or "neu" or "" && !string.IsNullOrWhiteSpace(result.Sentiment))
                await feedbackRepo.UpdateSentimentAsync(item.Id, result.Sentiment);

            // Upsert tags — reuse existing, create new
            var tagNames = result.Tags ?? [];
            var existingTags = await db.Tags.Where(t => tagNames.Contains(t.Name)).ToListAsync(ct);
            var existingNames = existingTags.Select(t => t.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newTags = tagNames
                .Where(n => !existingNames.Contains(n))
                .Select(n => new TagEntity { Name = n })
                .ToList();
            item.Tags = [.. existingTags, .. newTags];

            item.AiNote = result.AiNote ?? item.AiNote;

            // Link or create opportunity — lock guards the shared list across parallel tasks
            bool validMatch;
            lock (oppLock)
                validMatch = !string.IsNullOrWhiteSpace(result.MatchedOpportunityId)
                    && opportunities.Any(o => o.Id == result.MatchedOpportunityId);

            if (validMatch)
            {
                item.OpportunityId = result.MatchedOpportunityId;
            }
            else if (result.NewOpportunity is { Title.Length: > 0 })
            {
                var opp = new OpportunityEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = result.NewOpportunity.Title,
                    Sub = result.NewOpportunity.Sub ?? "",
                    Status = "open",
                    Tags = [.. existingTags, .. newTags]
                };
                db.Opportunities.Add(opp);
                item.OpportunityId = opp.Id;
                lock (oppLock) opportunities.Add(opp); // keep shared snapshot current
            }

            item.IsAiProcessed = true;
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Processed feedback {Id} — sentiment: {Sentiment}, tags: [{Tags}], opp: {Opp}",
                item.Id,
                item.Sentiment,
                string.Join(", ", tagNames),
                item.OpportunityId ?? "none");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process feedback {Id} — will retry next poll.", item.Id);
        }
    }

    private async Task<AiAnalysisResult?> CallClaudeAsync(
        FeedbackItemEntity item,
        List<OpportunityEntity> opportunities,
        CancellationToken ct)
    {
        var apiKey = configuration["Anthropic:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("Anthropic:ApiKey is not configured.");
            return null;
        }

        var model = configuration["Anthropic:Model"] ?? "claude-opus-4-6";
        var baseUrl = (configuration["Anthropic:BaseUrl"] ?? "https://api.anthropic.com").TrimEnd('/') + "/v1/messages";
        var apiVersion = configuration["Anthropic:ApiVersion"] ?? "2023-06-01";
        var maxTokens = configuration.GetValue("Anthropic:MaxTokens", 512);

        var oppList = opportunities.Count > 0
            ? string.Join("\n", opportunities.Select(o => $"- {o.Id}: {o.Title}"))
            : "(none yet)";

        var userPrompt = $$"""
            Analyze this customer feedback and return structured data.

            Feedback: "{{item.Text}}"
            Customer type: {{item.UserType}}
            Platform: {{item.Platform}}

            Existing opportunities (id → title):
            {{oppList}}

            Return exactly this JSON (no markdown, no prose):
            {
              "sentiment": "pos" | "neg" | "neu",
              "tags": ["tag1", "tag2"],
              "aiNote": "One-sentence insight.",
              "matchedOpportunityId": "<id>" | null,
              "newOpportunity": { "title": "...", "sub": "one-line description" } | null
            }

            Rules:
            - matchedOpportunityId and newOpportunity are mutually exclusive; set the unused one to null
            - Only create a new opportunity if no existing one clearly fits this feedback
            - Tags should be lowercase, specific product themes (2–4 max)
            """;

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            system = "You are an AI analyzing customer feedback for a product insights platform. " +
                     "Respond ONLY with a valid JSON object — no markdown, no code fences, no prose.",
            messages = new[] { new { role = "user", content = userPrompt } }
        };

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", apiVersion);

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(baseUrl, content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Anthropic API error {Status}: {Body}", response.StatusCode, responseBody);
            return null;
        }

        using var doc = JsonDocument.Parse(responseBody);
        var rawText = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? "";

        var cleanedJson = StripMarkdownFences(rawText);

        try
        {
            return JsonSerializer.Deserialize<AiAnalysisResult>(cleanedJson, JsonOpts);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to parse AI response JSON: {Raw}", rawText);
            return null;
        }
    }

    private static string StripMarkdownFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            var lastFence = trimmed.LastIndexOf("```");
            if (firstNewline > 0 && lastFence > firstNewline)
                return trimmed[(firstNewline + 1)..lastFence].Trim();
        }
        return trimmed;
    }
}

internal record AiAnalysisResult(
    [property: JsonPropertyName("sentiment")] string Sentiment,
    [property: JsonPropertyName("tags")] List<string>? Tags,
    [property: JsonPropertyName("aiNote")] string? AiNote,
    [property: JsonPropertyName("matchedOpportunityId")] string? MatchedOpportunityId,
    [property: JsonPropertyName("newOpportunity")] NewOpportunityResult? NewOpportunity
);

internal record NewOpportunityResult(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("sub")] string? Sub
);
