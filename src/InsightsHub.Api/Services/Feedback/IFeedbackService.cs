using InsightsHub.Api.Models;

namespace InsightsHub.Api.Services.Feedback;

public interface IFeedbackService
{
    Task<FeedbackPage> GetFeedbackAsync(
        int page, int pageSize,
        string? source, string? sentiment, string? theme, string? search);

    Task AddFeedbackAsync(AddFeedbackRequest req);

    Task<int> ImportFeedbackAsync(IEnumerable<AddFeedbackRequest> items);

    Task<FeedbackTrendsResult> GetTrendsAsync(int weeks = 10);

    Task<List<Tag>> SearchTagsAsync(string q);

    Task<Tag?> AddTagToFeedbackAsync(string feedbackId, string tagName);

    Task<bool> LinkOpportunityAsync(string feedbackId, string opportunityId);

    Task<List<FeedbackItem>> GetFeedbackForContextAsync(
        List<string>? tags, string? opportunityId, int limit = 20, DateTime? since = null);
}
