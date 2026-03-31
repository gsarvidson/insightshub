namespace InsightsHub.Api.Models;

public record DashboardMetrics(
    int TotalFeedback,
    string TotalFeedbackDelta,
    int OpenOpportunities,
    string OpenOpportunitiesDelta,
    int TrendingThemes,
    string TrendingThemesDelta,
    int AvgSentimentPct,
    string AvgSentimentDelta
);

public record TrendingTheme(
    string Label,
    int Count,
    string? TrendChip,
    string BarColor,
    double BarWidthPct,
    bool IsUrgent = false
);

public record DashboardAlert(
    string Text,
    string Time,
    string Color,
    string? NavTarget
);

public record SourceBreakdown(
    string Name,
    int Count,
    int Pct,
    string Color
);

public record DashboardSummary(
    DashboardMetrics Metrics,
    string AiSummary,
    List<TrendingTheme> TrendingThemes,
    List<DashboardAlert> Alerts,
    List<SourceBreakdown> SourceBreakdown,
    List<int> VolumeData,
    List<string> VolumeLabels
);
