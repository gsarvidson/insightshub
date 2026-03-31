using InsightsHub.Api.Models;
using InsightsHub.Api.Repositories.Dashboard;

namespace InsightsHub.Api.Services.Dashboard;

public class DashboardService(IDashboardRepository repository) : IDashboardService
{
    private static readonly Dictionary<string, string> SourceColors = new()
    {
        ["Salesforce"] = "#1A6BB5",
        ["CSAT"] = "#22C55E",
        ["NPS"] = "#F59E0B",
        ["Slack B2B"] = "#7C3AED",
    };

    public async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        var totalFeedback      = await repository.CountFeedbackAsync();
        var openOpportunities  = await repository.CountOpenOpportunitiesAsync();
        var distinctThemes     = await repository.CountDistinctThemesAsync();
        var sentimentPct       = await repository.GetPositiveSentimentPctAsync();
        var topThemes          = await repository.GetTopThemesAsync(5);
        var sourceCounts       = await repository.GetSourceCountsAsync();
        var (volumeCounts, volumeLabels) = await repository.GetWeeklyVolumeAsync(8);

        var maxThemeCount = topThemes.Count > 0 ? topThemes[0].Count : 1;
        var trendingThemes = topThemes
            .Select(t => new TrendingTheme(
                Label: t.Theme,
                Count: t.Count,
                TrendChip: null,
                BarColor: "#378ADD",
                BarWidthPct: maxThemeCount > 0 ? Math.Round(t.Count * 100.0 / maxThemeCount, 1) : 0))
            .ToList();

        var total = sourceCounts.Sum(s => s.Count);
        var sourceBreakdown = sourceCounts
            .Select(s => new SourceBreakdown(
                Name: s.Name,
                Count: s.Count,
                Pct: total > 0 ? (int)Math.Round(s.Count * 100.0 / total) : 0,
                Color: SourceColors.GetValueOrDefault(s.Name, "#5F5E5A")))
            .ToList();

        return new DashboardSummary(
            Metrics: new DashboardMetrics(
                TotalFeedback: totalFeedback,
                TotalFeedbackDelta: "",
                OpenOpportunities: openOpportunities,
                OpenOpportunitiesDelta: "",
                TrendingThemes: distinctThemes,
                TrendingThemesDelta: "",
                AvgSentimentPct: sentimentPct,
                AvgSentimentDelta: ""),
            AiSummary: "",
            TrendingThemes: trendingThemes,
            Alerts: [],
            SourceBreakdown: sourceBreakdown,
            VolumeData: volumeCounts,
            VolumeLabels: volumeLabels
        );
    }
}
