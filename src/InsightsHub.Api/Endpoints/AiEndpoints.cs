using InsightsHub.Api.Models;
using InsightsHub.Api.Services.Feedback;
using InsightsHub.Api.Services.Opportunity;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InsightsHub.Api.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this WebApplication app)
    {
        app.MapPost("/api/ai/chat", async (
            ChatRequest req,
            IFeedbackService feedbackService,
            IOpportunityService opportunityService,
            IHttpClientFactory clientFactory,
            IConfiguration config,
            ILogger<Program> logger) =>
        {
            var apiKey = config["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return Results.BadRequest(new ChatResponse("", false, "Anthropic API key is not configured. Set Anthropic:ApiKey in appsettings."));

            if (req.Messages.Count == 0)
                return Results.BadRequest(new ChatResponse("", false, "Messages cannot be empty."));

            var model = config["Anthropic:Model"] ?? "claude-opus-4-6";
            var baseUrl = (config["Anthropic:BaseUrl"] ?? "https://api.anthropic.com").TrimEnd('/') + "/v1/messages";
            var apiVersion = config["Anthropic:ApiVersion"] ?? "2023-06-01";
            var maxTokens = int.TryParse(config["Anthropic:MaxTokens"], out var mt) ? mt : 1024;

            var systemPrompt =
                "You are an AI assistant embedded in an internal product insights hub tool called \"Insights Hub\". " +
                "You help product managers analyse customer feedback data, identify trends, and make informed product decisions. " +
                "The platform aggregates feedback from Salesforce cases, CSAT surveys, NPS responses, and Slack B2B channels. " +
                "When live data context is provided below, base your answer on it. " +
                "Keep responses concise but specific; use bullet points or numbered lists where useful.";

            // Auto-build context from the last user message
            var lastUserMessage = req.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? "";
            if (!string.IsNullOrWhiteSpace(lastUserMessage))
            {
                // Detect temporal intent so we can scope to a date window
                var lowerMsg = lastUserMessage.ToLowerInvariant();
                DateTime? since = null;
                string? periodLabel = null;
                if (lowerMsg.Contains("today"))
                {
                    since = DateTime.UtcNow.Date;
                    periodLabel = "today";
                }
                else if (lowerMsg.Contains("this week") || lowerMsg.Contains("past week") || lowerMsg.Contains("last 7"))
                {
                    since = DateTime.UtcNow.AddDays(-7);
                    periodLabel = "the past 7 days";
                }
                else if (lowerMsg.Contains("last week"))
                {
                    since = DateTime.UtcNow.AddDays(-14);
                    periodLabel = "the past 14 days";
                }
                else if (lowerMsg.Contains("this month") || lowerMsg.Contains("past month") || lowerMsg.Contains("last 30"))
                {
                    since = DateTime.UtcNow.AddDays(-30);
                    periodLabel = "the past 30 days";
                }
                else if (lowerMsg.Contains("recent") || lowerMsg.Contains("latest") || lowerMsg.Contains("current") || lowerMsg.Contains("now"))
                {
                    since = DateTime.UtcNow.AddDays(-14);
                    periodLabel = "the past 14 days";
                }

                // Find tags in the DB whose names relate to the user's query
                var matchingTags = await feedbackService.SearchTagsAsync(lastUserMessage);
                var tagNames = matchingTags.Select(t => t.Name).ToList();

                List<FeedbackItem> contextFeedback;
                if (since.HasValue && tagNames.Count == 0)
                {
                    // Temporal query with no tag match — fetch all recent items in the date window
                    contextFeedback = await feedbackService.GetFeedbackForContextAsync(null, null, 20, since);
                }
                else if (tagNames.Count > 0)
                {
                    // Fetch feedback linked to those tags, optionally scoped to date window
                    contextFeedback = await feedbackService.GetFeedbackForContextAsync(tagNames, null, 15, since);
                }
                else
                {
                    // Full-text search fallback
                    var page = await feedbackService.GetFeedbackAsync(1, 15, null, null, null, lastUserMessage);
                    contextFeedback = page.Items;

                    // If still empty, pull the most recent items as a baseline
                    if (contextFeedback.Count == 0)
                        contextFeedback = await feedbackService.GetFeedbackForContextAsync(null, null, 15, DateTime.UtcNow.AddDays(-30));
                }

                // Also surface opportunities that match the query
                var allOpps = await opportunityService.GetOpportunitiesAsync(null);
                var relatedOpps = allOpps
                    .Where(o => lastUserMessage.Contains(o.Title, StringComparison.OrdinalIgnoreCase)
                             || (tagNames.Count > 0 && o.Tags.Any(t => tagNames.Contains(t.Name))))
                    .Take(3)
                    .ToList();

                if (contextFeedback.Count > 0 || relatedOpps.Count > 0)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("\n\n--- LIVE DATA CONTEXT ---");

                    if (periodLabel is not null)
                        sb.AppendLine($"Time period: {periodLabel}");

                    if (tagNames.Count > 0)
                        sb.AppendLine($"Relevant tags detected: {string.Join(", ", tagNames)}");

                    if (relatedOpps.Count > 0)
                    {
                        sb.AppendLine("\nRelated opportunities:");
                        foreach (var o in relatedOpps)
                        {
                            sb.AppendLine($"  • {o.Title} [{o.Status}] — {o.Mentions} mentions");
                            if (!string.IsNullOrWhiteSpace(o.AiNotes))
                                sb.AppendLine($"    AI Notes: {o.AiNotes}");
                        }
                    }

                    if (contextFeedback.Count > 0)
                    {
                        sb.AppendLine($"\nMatching feedback ({contextFeedback.Count} items, newest first):");
                        for (int i = 0; i < contextFeedback.Count; i++)
                        {
                            var f = contextFeedback[i];
                            var excerpt = f.Text.Length > 200 ? f.Text[..200] + "…" : f.Text;
                            var fTags = f.Themes.Count > 0 ? string.Join(", ", f.Themes.Select(t => t.Name)) : "none";
                            sb.AppendLine($"{i + 1}. [{f.Sentiment}] {excerpt}");
                            sb.AppendLine($"   Tags: {fTags} | Source: {f.Src} | {f.Date:yyyy-MM-dd}");
                        }
                    }

                    sb.AppendLine("--- END CONTEXT ---");
                    systemPrompt += sb.ToString();
                    logger.LogInformation("AI chat context: {TagCount} tags, {FeedbackCount} feedback, {OppCount} opps",
                        tagNames.Count, contextFeedback.Count, relatedOpps.Count);
                }
            }

            var anthropicBody = new
            {
                model,
                max_tokens = maxTokens,
                system = systemPrompt,
                messages = req.Messages.Select(m => new { role = m.Role, content = m.Content }).ToList()
            };

            try
            {
                var client = clientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", apiVersion);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(anthropicBody);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("Anthropic API error {Status}: {Body}", response.StatusCode, responseBody);
                    return Results.Ok(new ChatResponse("", false, $"Anthropic API error: {response.StatusCode}"));
                }

                using var doc = JsonDocument.Parse(responseBody);
                var text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                return Results.Ok(new ChatResponse(text, true));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calling Anthropic API");
                return Results.Ok(new ChatResponse("", false, "Failed to contact AI service."));
            }
        }).WithName("Chat");
    }
}
