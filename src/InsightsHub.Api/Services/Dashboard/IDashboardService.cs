using InsightsHub.Api.Models;

namespace InsightsHub.Api.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummary> GetDashboardSummaryAsync();
}
