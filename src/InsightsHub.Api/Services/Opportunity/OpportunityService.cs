using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InsightsHub.Api.Data.Entities;
using InsightsHub.Api.Models;
using InsightsHub.Api.Repositories.Opportunity;
using OpportunityModel = InsightsHub.Api.Models.Opportunity;

namespace InsightsHub.Api.Services.Opportunity;

public class OpportunityService(
    IOpportunityRepository repository,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OpportunityService> logger) : IOpportunityService
{
    private static readonly string[] ColorPalette =
    [
        "#6366f1", "#f59e0b", "#10b981", "#3b82f6",
        "#ec4899", "#8b5cf6", "#14b8a6", "#f97316",
        "#06b6d4", "#84cc16"
    ];
    public async Task<List<OpportunityModel>> GetOpportunitiesAsync(string? statusFilter)
    {
        var entities = await repository.GetAllAsync();
        var sourceCounts = await repository.GetSourceCountsByOpportunityAsync();
        var feedbackDates = await repository.GetFeedbackDatesByOpportunityAsync();
        var maxMentions = feedbackDates.Count > 0 ? feedbackDates.Values.Max(d => d.Count) : 0;
        var opportunities = entities.Select(e => Map(e, sourceCounts, feedbackDates, maxMentions)).ToList();

        if (!string.IsNullOrWhiteSpace(statusFilter))
            opportunities = opportunities.Where(o => o.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        return [.. opportunities.OrderByDescending(op => op.Mentions)];
    }

    public async Task<OpportunityModel?> GetOpportunityAsync(string id)
    {
        var entity = await repository.GetByIdAsync(id);
        if (entity is null) return null;
        var sourceCounts = await repository.GetSourceCountsByOpportunityAsync();
        var feedbackDates = await repository.GetFeedbackDatesByOpportunityAsync();
        var maxMentions = feedbackDates.Count > 0 ? feedbackDates.Values.Max(d => d.Count) : 0;
        return Map(entity, sourceCounts, feedbackDates, maxMentions);
    }

    public Task<bool> UpdateStatusAsync(string id, string status) =>
        repository.UpdateStatusAsync(id, status);

    public async Task<OpportunityModel> CreateOpportunityAsync(CreateOpportunityRequest req)
    {
        var count = await repository.CountAsync();
        var color = ColorPalette[count % ColorPalette.Length];

        var entity = new OpportunityEntity
        {
            Id = Guid.NewGuid().ToString(),
            Title = req.Title,
            Sub = req.Sub,
            Status = req.Status ?? "Backlog",
            Color = color,
        };

        var created = await repository.CreateAsync(entity, req.Tags);
        var sourceCounts = await repository.GetSourceCountsByOpportunityAsync();
        var feedbackDates = await repository.GetFeedbackDatesByOpportunityAsync();
        var maxMentions = feedbackDates.Count > 0 ? feedbackDates.Values.Max(d => d.Count) : 0;
        return Map(created, sourceCounts, feedbackDates, maxMentions);
    }

    public async Task RefreshAiNotesAsync(string opportunityId)
    {
        try
        {
            var entity = await repository.GetByIdAsync(opportunityId);
            if (entity is null) return;

            var texts = await repository.GetFeedbackTextsAsync(opportunityId);
            if (texts.Count == 0)
            {
                await repository.UpdateAiNotesAsync(opportunityId, "No feedback linked yet.");
                return;
            }

            var apiKey = configuration["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                await repository.UpdateAiNotesAsync(opportunityId, $"AI analysis pending — {texts.Count} feedback item(s) linked.");
                return;
            }

            var model = configuration["Anthropic:Model"] ?? "claude-opus-4-6";
            var baseUrl = (configuration["Anthropic:BaseUrl"] ?? "https://api.anthropic.com").TrimEnd('/') + "/v1/messages";
            var apiVersion = configuration["Anthropic:ApiVersion"] ?? "2023-06-01";

            var excerpts = string.Join("\n", texts.Take(20).Select((t, i) => $"{i + 1}. {t}"));
            var prompt =
                $"You are analysing customer feedback for a product opportunity titled \"{entity.Title}\".\n\n" +
                $"Feedback items ({texts.Count} total, showing up to 20):\n{excerpts}\n\n" +
                "Provide a concise 2–3 sentence AI summary covering: the key themes in this feedback, " +
                "the severity or urgency of the issue, and one actionable insight for the product team. " +
                "Be specific and direct.";

            var body = JsonSerializer.Serialize(new
            {
                model,
                max_tokens = 256,
                messages = new[] { new { role = "user", content = prompt } }
            });

            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", apiVersion);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(baseUrl, new StringContent(body, Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Anthropic API error {Status} refreshing AiNotes for {Id}", response.StatusCode, opportunityId);
                return;
            }

            using var doc = JsonDocument.Parse(responseBody);
            var aiNotes = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "";

            await repository.UpdateAiNotesAsync(opportunityId, aiNotes);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to refresh AiNotes for opportunity {Id}", opportunityId);
        }
    }

    private static OpportunityModel Map(
        OpportunityEntity e,
        Dictionary<string, List<(string Name, int Count)>> sourceCounts,
        Dictionary<string, List<DateTime>> feedbackDates,
        int maxMentions)
    {
        var sources = sourceCounts.TryGetValue(e.Id, out var sc)
            ? sc.Select(s => new SourceCount(s.Name, s.Count)).ToList()
            : [];

        var dates = feedbackDates.TryGetValue(e.Id, out var fd) ? fd : [];

        var now = DateTime.UtcNow;
        var recentCutoff = now.AddDays(-30);
        var priorCutoff = now.AddDays(-60);

        var recentCount = dates.Count(d => d >= recentCutoff);
        var priorCount = dates.Count(d => d >= priorCutoff && d < recentCutoff);

        var trend = recentCount > priorCount ? "up"
                  : recentCount < priorCount ? "down"
                  : "stable";

        var trendColor = trend switch
        {
            "up" => "#22c55e",
            "down" => "#ef4444",
            _ => "#64748b"
        };

        var scorePercent = maxMentions > 0 ? Math.Round((double)dates.Count / maxMentions * 100, 1) : 0;

        var trendBars = ComputeWeeklyBars(dates, now, weeks: 10);

        var color = string.IsNullOrEmpty(e.Color)
            ? ColorPalette[Math.Abs(e.Id.GetHashCode()) % ColorPalette.Length]
            : e.Color;

        return new OpportunityModel(
            Id: e.Id,
            Title: e.Title,
            Sub: e.Sub,
            Status: e.Status,
            Mentions: dates.Count,
            Trend: trend,
            TrendColor: trendColor,
            ScorePercent: scorePercent,
            Sources: sources,
            Tags: e.Tags.Select(t => new Tag(t.Name, TagColor(t))).ToList(),
            TrendBars: trendBars,
            Teams: e.Teams.Select(t => t.Name).ToList(),
            Color: color,
            AiNotes: e.AiNotes
        );
    }

    private static string TagColor(TagEntity t) =>
        string.IsNullOrEmpty(t.Color)
            ? ColorPalette[Math.Abs(t.Id) % ColorPalette.Length]
            : t.Color;

    private static List<int> ComputeWeeklyBars(List<DateTime> dates, DateTime now, int weeks)
    {
        var counts = Enumerable.Range(0, weeks)
            .Select(i =>
            {
                var weekEnd = now.AddDays(-i * 7);
                var weekStart = weekEnd.AddDays(-7);
                return dates.Count(d => d >= weekStart && d < weekEnd);
            })
            .Reverse()
            .ToList();

        var max = counts.Max();
        return max == 0
            ? counts
            : counts.Select(c => (int)Math.Round((double)c / max * 40)).ToList();
    }
}
