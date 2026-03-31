using InsightsHub.Api.Data;
using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightsHub.Api.Repositories.Opportunity;

public class OpportunityRepository(InsightsHubDbContext context) : BaseRepository(context), IOpportunityRepository
{
    public Task<List<OpportunityEntity>> GetAllAsync() =>
        Context.Opportunities
            .Include(o => o.Tags)
            .Include(o => o.Teams)
            .ToListAsync();

    public Task<OpportunityEntity?> GetByIdAsync(string id) =>
        Context.Opportunities
            .Include(o => o.Tags)
            .Include(o => o.Teams)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<bool> UpdateStatusAsync(string id, string status)
    {
        var entity = await Context.Opportunities.FindAsync(id);
        if (entity is null) return false;
        entity.Status = status;
        await Context.SaveChangesAsync();
        return true;
    }

    public Task<int> CountAsync() =>
        Context.Opportunities.CountAsync();

    public async Task<bool> UpdateAiNotesAsync(string id, string aiNotes)
    {
        var entity = await Context.Opportunities.FindAsync(id);
        if (entity is null) return false;
        entity.AiNotes = aiNotes;
        await Context.SaveChangesAsync();
        return true;
    }

    public Task<List<string>> GetFeedbackTextsAsync(string opportunityId) =>
        Context.FeedbackItems
            .Where(f => f.OpportunityId == opportunityId)
            .Select(f => f.Text)
            .ToListAsync();

    public async Task<OpportunityEntity> CreateAsync(OpportunityEntity entity, IEnumerable<string>? tagNames = null)
    {
        if (tagNames is not null)
        {
            var names = tagNames.ToList();
            var existing = await Context.Tags.Where(t => names.Contains(t.Name)).ToListAsync();
            var existingNames = existing.Select(t => t.Name).ToHashSet();
            var newTags = names.Where(n => !existingNames.Contains(n)).Select(n => new TagEntity { Name = n });
            entity.Tags = [.. existing, .. newTags];
        }

        Context.Opportunities.Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public async Task<Dictionary<string, List<(string Name, int Count)>>> GetSourceCountsByOpportunityAsync()
    {
        var rows = await Context.FeedbackItems
            .Where(f => f.OpportunityId != null)
            .SelectMany(f => f.DataSources, (f, d) => new { f.OpportunityId, SourceName = d.Name })
            .GroupBy(x => new { x.OpportunityId, x.SourceName })
            .Select(g => new { g.Key.OpportunityId, g.Key.SourceName, Count = g.Count() })
            .ToListAsync();

        return rows
            .GroupBy(r => r.OpportunityId!)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => (r.SourceName, r.Count)).ToList()
            );
    }

    public async Task<Dictionary<string, List<DateTime>>> GetFeedbackDatesByOpportunityAsync()
    {
        var rows = await Context.FeedbackItems
            .Where(f => f.OpportunityId != null)
            .Select(f => new { f.OpportunityId, f.Date })
            .ToListAsync();

        return rows
            .GroupBy(r => r.OpportunityId!)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Date).ToList());
    }
}
