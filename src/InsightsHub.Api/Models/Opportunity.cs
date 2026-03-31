namespace InsightsHub.Api.Models;

public record SourceCount(string Name, int Count);
public record Tag(string Name, string Color);

public record Opportunity(
    string Id,
    string Title,
    string Sub,
    string Status,
    int Mentions,
    string Trend,
    string TrendColor,
    double ScorePercent,
    List<SourceCount> Sources,
    List<Tag> Tags,
    List<int> TrendBars,
    List<string> Teams,
    string Color,
    string? AiNotes
);

public record UpdateStatusRequest(string Status);

public record CreateOpportunityRequest(
    string Title,
    string Sub,
    string? Status,
    List<string>? Tags
);
