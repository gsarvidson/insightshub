using InsightsHub.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace InsightsHub.Api.Repositories.Dashboard;

public class DashboardRepository(InsightsHubDbContext context) : BaseRepository(context), IDashboardRepository
{
    public Task<int> CountFeedbackAsync() =>
        Context.FeedbackItems.CountAsync();

    public Task<int> CountOpenOpportunitiesAsync() =>
        Context.Opportunities.CountAsync(o => o.Status.ToLower() == "open");

    public async Task<int> CountDistinctThemesAsync()
    {
        var allThemes = await Context.FeedbackItems
            .Select(f => f.Tags)
            .ToListAsync();

        return allThemes.SelectMany(t => t.Select(te => te.Name)).Distinct().Count();
    }

    public async Task<int> GetPositiveSentimentPctAsync()
    {
        var total = await Context.FeedbackItems.CountAsync();
        if (total == 0) return 0;
        var positive = await Context.FeedbackItems.CountAsync(f => f.Sentiment == "pos");
        return (int)Math.Round(positive * 100.0 / total);
    }

    public async Task<List<(string Theme, int Count)>> GetTopThemesAsync(int take)
    {
        var allThemes = await Context.FeedbackItems
            .Select(f => f.Tags)
            .ToListAsync();

        return allThemes
            .SelectMany(t => t.Select(te => te.Name))
            .GroupBy(t => t)
            .Select(g => (Theme: g.Key, Count: g.Count()))
            .OrderByDescending(t => t.Count)
            .Take(take)
            .ToList();
    }

    public async Task<List<(string Name, int Count)>> GetSourceCountsAsync()
    {
        var groups = await Context.DataSources
            .Select(d => new { d.Name, Count = d.FeedbackItems.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        return groups.Select(g => (g.Name, g.Count)).ToList();
    }

    public async Task<(List<int> Counts, List<string> Labels)> GetWeeklyVolumeAsync(int weeks)
    {
        var cutoff = DateTime.UtcNow.AddDays(-weeks * 7);
        var createdDates = await Context.FeedbackItems
            .Where(f => f.CreatedAt >= cutoff)
            .Select(f => f.CreatedAt)
            .ToListAsync();

        var counts = new List<int>();
        var labels = new List<string>();
        for (var i = weeks; i >= 1; i--)
        {
            var weekStart = DateTime.UtcNow.AddDays(-i * 7);
            var weekEnd = weekStart.AddDays(7);
            counts.Add(createdDates.Count(d => d >= weekStart && d < weekEnd));
            labels.Add($"W{weeks - i + 1}");
        }

        return (counts, labels);
    }
}
