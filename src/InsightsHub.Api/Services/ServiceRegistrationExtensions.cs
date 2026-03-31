using InsightsHub.Api.Services.Dashboard;
using InsightsHub.Api.Services.Feedback;
using InsightsHub.Api.Services.Opportunity;
using InsightsHub.Api.Services.Sources;

namespace InsightsHub.Api.Services;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddInsightsHubServices(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ISourcesService, SourcesService>();
        return services;
    }
}
