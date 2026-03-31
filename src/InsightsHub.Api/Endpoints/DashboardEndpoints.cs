using InsightsHub.Api.Services.Dashboard;

namespace InsightsHub.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/dashboard", async (IDashboardService svc) =>
            Results.Ok(await svc.GetDashboardSummaryAsync()))
            .WithName("GetDashboard");
    }
}
