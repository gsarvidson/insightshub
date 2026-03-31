namespace InsightsHub.Api.Models;

public record FeedbackItem(
    string Id,
    string Text,
    string Meta,
    string Src,
    string SrcLabel,
    DateTime Date,
    string Sentiment,
    string SentimentColor,
    List<Tag> Themes,
    string Opp,
    string OppKey,
    string UserType,
    string Platform,
    string AiNote,
    List<string> Teams
);

public record AddFeedbackRequest(
    string Text,
    string Source,
    string? CustomerType,
    string? CustomerIdentifier,
    DateTime? Date,
    string? Sentiment,
    string? OppKey,
    List<string>? Tags,
    string? Platform,
    string? Notes,
    string? Team
);

public record FeedbackPage(List<FeedbackItem> Items, int Total, int Page, int PageSize);

public record AddTagRequest(string Name);
public record LinkOpportunityRequest(string OppId);

public record FeedbackOptionItem(string Id, string Title);
public record FeedbackOptions(
    string[] Sources,
    string[] CustomerTypes,
    string[] Verticals,
    FeedbackOptionItem[] Opportunities);

public record TagWeeklyData(string Label, int[] Data);
public record FeedbackTrendsResult(string[] Labels, List<TagWeeklyData> Series);
