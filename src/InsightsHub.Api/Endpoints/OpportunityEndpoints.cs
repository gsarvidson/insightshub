using InsightsHub.Api.Models;
using InsightsHub.Api.Services.Opportunity;

namespace InsightsHub.Api.Endpoints;

public static class OpportunityEndpoints
{
    public static void MapOpportunityEndpoints(this WebApplication app)
    {
        app.MapGet("/api/opportunities", async (IOpportunityService svc, string? status) =>
        {
            var opps = await svc.GetOpportunitiesAsync(status == "All" ? null : status);
            return Results.Ok(opps);
        }).WithName("GetOpportunities");

        app.MapGet("/api/opportunities/{id}", async (IOpportunityService svc, string id) =>
        {
            var opp = await svc.GetOpportunityAsync(id);
            return opp is null ? Results.NotFound() : Results.Ok(opp);
        }).WithName("GetOpportunity");

        app.MapPost("/api/opportunities", async (IOpportunityService svc, CreateOpportunityRequest req) =>
        {
            if (string.IsNullOrWhiteSpace(req.Title))
                return Results.BadRequest(new { error = "Title is required" });

            var created = await svc.CreateOpportunityAsync(req);
            return Results.Created($"/api/opportunities/{created.Id}", created);
        }).WithName("CreateOpportunity");

        app.MapPatch("/api/opportunities/{id}/status", async (IOpportunityService svc, string id, UpdateStatusRequest req) =>
        {
            var validStatuses = new[] { "Urgent", "Under review", "On roadmap", "Backlog", "Done" };
            if (!validStatuses.Contains(req.Status))
                return Results.BadRequest(new { error = "Invalid status value" });

            var updated = await svc.UpdateStatusAsync(id, req.Status);
            return updated ? Results.Ok(new { id, status = req.Status }) : Results.NotFound();
        }).WithName("UpdateOpportunityStatus");
    }
}
