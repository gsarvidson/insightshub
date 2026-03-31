using InsightsHub.Api.Data;
using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightsHub.Api.Repositories.Feedback;

public class FeedbackRepository(InsightsHubDbContext context) : BaseRepository(context), IFeedbackRepository
{
    public async Task<(List<FeedbackItemEntity> Items, int Total)> GetPagedAsync(
        int page, int pageSize,
        string? src, string? sentiment, string? theme, string? search, string? oppId = null)
    {
        var query = Context.FeedbackItems
            .Include(f => f.DataSources)
            .Include(f => f.Tags)
            .Include(f => f.Teams)
            .Include(f => f.Opportunity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(src))
            query = query.Where(f => f.DataSources.Any(d => d.Name.ToLower() == src.ToLower()));

        if (!string.IsNullOrWhiteSpace(sentiment))
            query = query.Where(f => f.Sentiment != null && f.Sentiment.ToLower() == sentiment.ToLower());

        if (!string.IsNullOrWhiteSpace(theme))
            query = query.Where(f => f.Tags.Any(t => t.Name == theme));

        if (!string.IsNullOrWhiteSpace(oppId))
            query = query.Where(f => f.OpportunityId == oppId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f =>
                EF.Functions.ILike(f.Text, $"%{search}%") ||
                EF.Functions.ILike(f.Meta, $"%{search}%"));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(string[] Labels, Dictionary<string, int[]> TagCounts)> GetTrendsAsync(int weeks = 10)
    {
        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(-weeks * 7);

        var items = await Context.FeedbackItems
            .Include(f => f.Tags)
            .Where(f => f.Date >= cutoff)
            .Select(f => new { f.Date, Tags = f.Tags.Select(t => t.Name).ToList() })
            .ToListAsync();

        var labels = Enumerable.Range(1, weeks).Select(i => $"W{i}").ToArray();

        var tagCounts = new Dictionary<string, int[]>();
        foreach (var item in items)
        {
            var daysAgo = (today - item.Date.Date).TotalDays;
            var bucketFromOldest = weeks - 1 - (int)(daysAgo / 7);
            if (bucketFromOldest < 0 || bucketFromOldest >= weeks) continue;

            foreach (var tag in item.Tags)
            {
                if (!tagCounts.ContainsKey(tag))
                    tagCounts[tag] = new int[weeks];
                tagCounts[tag][bucketFromOldest]++;
            }
        }

        return (labels, tagCounts);
    }

    public async Task<bool> UpdateSentimentAsync(string id, string sentiment)
    {
        var entity = await Context.FeedbackItems.FindAsync(id);
        if (entity is null) return false;
        entity.Sentiment = sentiment;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TagEntity>> SearchTagsAsync(string q, int limit = 10) =>
        await Context.Tags
            .Where(t => EF.Functions.ILike(t.Name, $"%{q}%"))
            .OrderBy(t => t.Name)
            .Take(limit)
            .ToListAsync();

    public async Task<TagEntity?> AddTagToFeedbackAsync(string feedbackId, string tagName)
    {
        var feedback = await Context.FeedbackItems
            .Include(f => f.Tags)
            .FirstOrDefaultAsync(f => f.Id == feedbackId);

        if (feedback is null) return null;
        if (feedback.Tags.Any(t => t.Name == tagName)) return feedback.Tags.First(t => t.Name == tagName);

        var tag = await Context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
        if (tag is null) return null;

        feedback.Tags.Add(tag);
        await Context.SaveChangesAsync();
        return tag;
    }

    public async Task<bool> LinkOpportunityAsync(string feedbackId, string opportunityId)
    {
        var entity = await Context.FeedbackItems.FindAsync(feedbackId);
        if (entity is null) return false;
        entity.OpportunityId = opportunityId;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<List<FeedbackItemEntity>> GetForAiContextAsync(
        List<string>? tags, string? opportunityId, int limit = 20, DateTime? since = null)
    {
        var query = Context.FeedbackItems
            .Include(f => f.Tags)
            .Include(f => f.DataSources)
            .Include(f => f.Opportunity)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(opportunityId))
            query = query.Where(f => f.OpportunityId == opportunityId);
        else if (tags is { Count: > 0 })
            query = query.Where(f => f.Tags.Any(t => tags.Contains(t.Name)));

        if (since.HasValue)
            query = query.Where(f => f.Date >= since.Value);

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<FeedbackItemEntity> AddAsync(FeedbackItemEntity entity, IEnumerable<string>? themeNames = null, string? sourceName = null, string? teamName = null)
    {
        if (themeNames is not null)
        {
            var names = themeNames.ToList();
            var existing = await Context.Tags.Where(t => names.Contains(t.Name)).ToListAsync();
            var existingNames = existing.Select(t => t.Name).ToHashSet();
            var newTags = names.Where(n => !existingNames.Contains(n)).Select(n => new TagEntity { Name = n });
            entity.Tags = [.. existing, .. newTags];
        }

        if (!string.IsNullOrWhiteSpace(sourceName))
        {
            var source = await Context.DataSources.FirstOrDefaultAsync(d => d.Name == sourceName);
            if (source is not null)
                entity.DataSources = [source];
        }

        if (!string.IsNullOrWhiteSpace(teamName))
        {
            var team = await Context.Teams.FirstOrDefaultAsync(t => t.Name == teamName);
            if (team is not null)
                entity.Teams = [team];
        }

        Context.FeedbackItems.Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }
}
