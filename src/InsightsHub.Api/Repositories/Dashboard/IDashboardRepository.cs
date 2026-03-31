namespace InsightsHub.Api.Repositories.Dashboard;

public interface IDashboardRepository
{
    Task<int> CountFeedbackAsync();
    Task<int> CountOpenOpportunitiesAsync();
    Task<int> CountDistinctThemesAsync();
    Task<int> GetPositiveSentimentPctAsync();
    Task<List<(string Theme, int Count)>> GetTopThemesAsync(int take);
    Task<List<(string Name, int Count)>> GetSourceCountsAsync();
    Task<(List<int> Counts, List<string> Labels)> GetWeeklyVolumeAsync(int weeks);
}
