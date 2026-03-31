using InsightsHub.Api.Repositories.Dashboard;
using InsightsHub.Api.Repositories.Feedback;
using InsightsHub.Api.Repositories.Opportunity;
using InsightsHub.Api.Repositories.Sources;

namespace InsightsHub.Api.Repositories;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddInsightsHubRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<ISourcesRepository, SourcesRepository>();
        return services;
    }
}
