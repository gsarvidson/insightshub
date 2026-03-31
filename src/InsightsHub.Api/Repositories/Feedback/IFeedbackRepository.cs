using InsightsHub.Api.Data.Entities;

namespace InsightsHub.Api.Repositories.Feedback;

public interface IFeedbackRepository
{
    Task<(List<FeedbackItemEntity> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        string? src, string? sentiment, string? theme, string? search, string? oppId = null);

    Task<FeedbackItemEntity> AddAsync(FeedbackItemEntity entity, IEnumerable<string>? themeNames = null, string? sourceName = null, string? teamName = null);

    Task<bool> UpdateSentimentAsync(string id, string sentiment);

    Task<(string[] Labels, Dictionary<string, int[]> TagCounts)> GetTrendsAsync(int weeks = 10);

    Task<List<TagEntity>> SearchTagsAsync(string q, int limit = 10);

    Task<TagEntity?> AddTagToFeedbackAsync(string feedbackId, string tagName);

    Task<bool> LinkOpportunityAsync(string feedbackId, string opportunityId);

    Task<List<FeedbackItemEntity>> GetForAiContextAsync(
        List<string>? tags, string? opportunityId, int limit = 20, DateTime? since = null);
}
