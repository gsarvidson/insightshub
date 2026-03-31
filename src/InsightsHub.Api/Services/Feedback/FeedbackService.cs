using InsightsHub.Api.Data.Entities;
using InsightsHub.Api.Models;
using InsightsHub.Api.Repositories.Feedback;
using InsightsHub.Api.Services.Opportunity;
using Tag = InsightsHub.Api.Models.Tag;

namespace InsightsHub.Api.Services.Feedback;

public class FeedbackService(
    IFeedbackRepository repository,
    IOpportunityService opportunityService) : IFeedbackService
{
    public async Task<FeedbackPage> GetFeedbackAsync(
        int page, int pageSize,
        string? source, string? sentiment, string? theme, string? search, string? oppId = null)
    {
        var (entities, total) = await repository.GetPagedAsync(page, pageSize, source, sentiment, theme, search, oppId);
        var items = entities.Select(Map).ToList();
        return new FeedbackPage(items, total, page, pageSize);
    }

    public async Task<FeedbackTrendsResult> GetTrendsAsync(int weeks = 10)
    {
        var (labels, tagCounts) = await repository.GetTrendsAsync(weeks);
        var series = tagCounts
            .OrderByDescending(kv => kv.Value.Sum())
            .Take(8)
            .Select(kv => new TagWeeklyData(kv.Key, kv.Value))
            .ToList();
        return new FeedbackTrendsResult(labels, series);
    }

    public async Task AddFeedbackAsync(AddFeedbackRequest req)
    {
        var id = Guid.NewGuid().ToString();

        var entity = new FeedbackItemEntity
        {
            Id = id,
            Text = req.Text,
            Meta = $"Manual · {req.CustomerType ?? "Unknown"}",
            Date = req.Date ?? DateTime.UtcNow,
            Sentiment = null,
            OpportunityId = string.IsNullOrEmpty(req.OppKey) ? null : req.OppKey,
            UserType = req.CustomerType ?? "Unknown",
            Platform = req.Platform ?? "Unknown",
            AiNote = "Manually added — AI analysis pending.",
        };

        await repository.AddAsync(entity, req.Tags, req.Source, req.Team);

        if (!string.IsNullOrEmpty(req.OppKey))
            await opportunityService.RefreshAiNotesAsync(req.OppKey);
    }

    public async Task<int> ImportFeedbackAsync(IEnumerable<AddFeedbackRequest> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            await AddFeedbackAsync(item);
            count++;
        }
        return count;
    }

    public async Task<List<Tag>> SearchTagsAsync(string q) =>
        (await repository.SearchTagsAsync(q))
            .Select(t => new Tag(t.Name, TagColor(t)))
            .ToList();

    public async Task<bool> LinkOpportunityAsync(string feedbackId, string opportunityId)
    {
        var linked = await repository.LinkOpportunityAsync(feedbackId, opportunityId);
        if (linked)
            await opportunityService.RefreshAiNotesAsync(opportunityId);
        return linked;
    }

    public async Task<List<FeedbackItem>> GetFeedbackForContextAsync(
        List<string>? tags, string? opportunityId, int limit = 20, DateTime? since = null)
    {
        var entities = await repository.GetForAiContextAsync(tags, opportunityId, limit, since);
        return entities.Select(Map).ToList();
    }

    public async Task<Tag?> AddTagToFeedbackAsync(string feedbackId, string tagName)
    {
        var tag = await repository.AddTagToFeedbackAsync(feedbackId, tagName);
        return tag is null ? null : new Tag(tag.Name, TagColor(tag));
    }

    private static readonly string[] ColorPalette =
    [
        "#6366f1", "#f59e0b", "#10b981", "#3b82f6",
        "#ec4899", "#8b5cf6", "#14b8a6", "#f97316",
        "#06b6d4", "#84cc16"
    ];

    private static string TagColor(TagEntity t) =>
        string.IsNullOrEmpty(t.Color)
            ? ColorPalette[Math.Abs(t.Id) % ColorPalette.Length]
            : t.Color;

    private static FeedbackItem Map(FeedbackItemEntity e) => new(
        Id: e.Id,
        Text: e.Text,
        Meta: e.Meta,
        Src: e.DataSources.FirstOrDefault()?.Name ?? "",
        SrcLabel: e.DataSources.FirstOrDefault()?.Name ?? "",
        Date: e.Date,
        Sentiment: e.Sentiment ?? "neu",
        SentimentColor: e.Sentiment == "pos" ? "#3B6D11" : e.Sentiment == "neg" ? "#A32D2D" : "#5F5E5A",
        Themes: e.Tags.Select(t => new Tag(t.Name, TagColor(t))).ToList(),
        Opp: e.Opportunity?.Title ?? "",
        OppKey: e.OpportunityId ?? "",
        UserType: e.UserType,
        Platform: e.Platform,
        AiNote: e.AiNote,
        Teams: e.Teams.Select(t => t.Name).ToList()
    );
}
