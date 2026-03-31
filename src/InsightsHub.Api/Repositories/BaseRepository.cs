using InsightsHub.Api.Data;

namespace InsightsHub.Api.Repositories;

public class BaseRepository(InsightsHubDbContext context)
{
    protected InsightsHubDbContext Context { get; } = context;
}
