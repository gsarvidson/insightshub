using InsightsHub.Api.Services.Sources;

namespace InsightsHub.Api.Endpoints;

public static class SourcesEndpoints
{
    public static void MapSourcesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/sources", async (ISourcesService svc) =>
        {
            var (sources, savedViews) = await svc.GetSourcesAsync();
            return Results.Ok(new { sources, savedViews });
        }).WithName("GetSources");

        app.MapPost("/api/sources/{id}/sync", async (string id, ISourcesService svc) =>
            await svc.SyncSourceAsync(id) ? Results.Ok() : Results.NotFound())
            .WithName("SyncSource");

        app.MapDelete("/api/sources/{id}", async (string id, ISourcesService svc) =>
            await svc.DisconnectSourceAsync(id) ? Results.NoContent() : Results.NotFound())
            .WithName("DisconnectSource");
    }
}
