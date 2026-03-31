namespace InsightsHub.Api.Models;

public record DataSource(
    string Id,
    string Name,
    string LastSynced,
    string Description,
    string Status
);

public record SavedView(
    string Name,
    string Meta
);
