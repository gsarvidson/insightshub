using InsightsHub.Api.Data;
using InsightsHub.Api.Models;
using InsightsHub.Api.Services.Feedback;
using Microsoft.EntityFrameworkCore;

namespace InsightsHub.Api.Endpoints;

public static class FeedbackEndpoints
{
    public static void MapFeedbackEndpoints(this WebApplication app)
    {
        app.MapGet("/api/feedback", async (IFeedbackService svc,
            int page = 1, int pageSize = 10,
            string? source = null, string? sentiment = null,
            string? theme = null, string? search = null, string? opp = null) =>
        {
            var result = await svc.GetFeedbackAsync(page, pageSize, source, sentiment, theme, search, opp);
            return Results.Ok(result);
        }).WithName("GetFeedback");

        app.MapGet("/api/feedback/trends", async (IFeedbackService svc, int weeks = 10) =>
        {
            var result = await svc.GetTrendsAsync(weeks);
            return Results.Ok(result);
        }).WithName("GetFeedbackTrends");

        app.MapPost("/api/feedback", async (IFeedbackService svc, AddFeedbackRequest req) =>
        {
            if (string.IsNullOrWhiteSpace(req.Text))
                return Results.BadRequest(new { error = "Feedback text is required" });
            if (string.IsNullOrWhiteSpace(req.Source))
                return Results.BadRequest(new { error = "Source is required" });

            await svc.AddFeedbackAsync(req);
            return Results.Created("/api/feedback", new { success = true });
        }).WithName("AddFeedback");

        app.MapGet("/api/tags/search", async (IFeedbackService svc, string q = "") =>
        {
            if (q.Length < 3) return Results.Ok(Array.Empty<object>());
            var tags = await svc.SearchTagsAsync(q);
            return Results.Ok(tags);
        }).WithName("SearchTags");

        app.MapPost("/api/feedback/{id}/tags", async (IFeedbackService svc, string id, AddTagRequest req) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Tag name is required" });
            var tag = await svc.AddTagToFeedbackAsync(id, req.Name);
            return tag is null ? Results.NotFound() : Results.Ok(tag);
        }).WithName("AddTagToFeedback");

        app.MapMethods("/api/feedback/{id}/opportunity", ["PATCH"], async (IFeedbackService svc, string id, LinkOpportunityRequest req) =>
        {
            if (string.IsNullOrWhiteSpace(req.OppId))
                return Results.BadRequest(new { error = "oppId is required" });
            var linked = await svc.LinkOpportunityAsync(id, req.OppId);
            return linked ? Results.Ok(new { success = true }) : Results.NotFound();
        }).WithName("LinkFeedbackOpportunity");

        app.MapGet("/api/feedback/by-opportunity/{oppId}", async (IFeedbackService svc, string oppId, int limit = 5) =>
        {
            var items = await svc.GetFeedbackForContextAsync(null, oppId, limit);
            return Results.Ok(items);
        }).WithName("GetFeedbackByOpportunity");

        app.MapGet("/api/feedback/options", async (InsightsHubDbContext db) =>
        {
            var sources = await db.DataSources
                .OrderBy(x => x.SortOrder)
                .Select(x => x.Name)
                .ToArrayAsync();

            var verticals = await db.Teams
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .ToArrayAsync();

            var opportunities = await db.Opportunities
                .OrderBy(x => x.Title)
                .Select(x => new FeedbackOptionItem(x.Id, x.Title))
                .ToArrayAsync();

            string[] customerTypes = ["Consumer", "Agent", "Dealer", "Developer", "Internal"];

            return Results.Ok(new FeedbackOptions(sources, customerTypes, verticals, opportunities));
        }).WithName("GetFeedbackOptions");

        app.MapPost("/api/feedback/import", async (IFeedbackService svc, HttpRequest request) =>
        {
            if (!request.HasFormContentType || request.Form.Files.Count == 0)
                return Results.BadRequest(new { error = "A CSV file is required." });

            var file = request.Form.Files[0];
            var source = request.Form.TryGetValue("source", out var sv) ? sv.ToString() : "Manual";

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var rows = ParseCsvRows(content);
            if (rows.Count == 0)
                return Results.BadRequest(new { error = "No data rows found in CSV." });

            var requests = rows
                .Select(r => MapCsvRow(r, source))
                .Where(r => !string.IsNullOrWhiteSpace(r.Text))
                .ToList();

            var imported = await svc.ImportFeedbackAsync(requests);
            return Results.Ok(new { imported, skipped = rows.Count - imported });
        }).WithName("ImportFeedback").DisableAntiforgery();
    }

    // Parses CSV into a list of dictionaries keyed by header name.
    private static List<Dictionary<string, string>> ParseCsvRows(string csv)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return [];

        var headers = SplitCsvLine(lines[0].Trim());
        var result = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            var values = SplitCsvLine(line);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int j = 0; j < headers.Count; j++)
                row[headers[j]] = j < values.Count ? values[j] : string.Empty;
            result.Add(row);
        }
        return result;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields;
    }

    private static AddFeedbackRequest MapCsvRow(Dictionary<string, string> row, string source)
    {
        var text      = Get(row, "Feedback");
        var userType  = Get(row, "User type");
        var vertical  = Get(row, "Vertical");
        var dateRaw   = Get(row, "Created date");
        var who       = Get(row, "Who raised it");
        var impact    = Get(row, "Impact");
        var submitter = Get(row, "Submitted by");

        DateTime? date = DateTime.TryParse(dateRaw, out var d) ? d : null;

        var tags = string.IsNullOrWhiteSpace(impact)
            ? null
            : new List<string> { impact.Trim() };

        var notes = string.IsNullOrWhiteSpace(submitter)
            ? null
            : $"Submitted by: {submitter.Trim()}";

        return new AddFeedbackRequest(
            Text: text,
            Source: source,
            CustomerType: string.IsNullOrWhiteSpace(userType) ? null : userType,
            CustomerIdentifier: string.IsNullOrWhiteSpace(who) ? null : who,
            Date: date,
            Sentiment: null,
            OppKey: null,
            Tags: tags,
            Platform: null,
            Notes: notes,
            Team: string.IsNullOrWhiteSpace(vertical) ? null : vertical
        );
    }

    private static string Get(Dictionary<string, string> row, string key) =>
        row.TryGetValue(key, out var v) ? v.Trim() : string.Empty;
}
