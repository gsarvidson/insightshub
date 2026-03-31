using InsightsHub.Api.Data;
using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightsHub.Api.Repositories.Sources;

public class SourcesRepository(InsightsHubDbContext context) : BaseRepository(context), ISourcesRepository
{
    public Task<List<DataSourceEntity>> GetAllSourcesAsync() =>
        Context.DataSources.OrderBy(s => s.SortOrder).ToListAsync();

    public Task<List<SavedViewEntity>> GetAllSavedViewsAsync() =>
        Context.SavedViews.OrderBy(s => s.Id).ToListAsync();

    public async Task<bool> SyncSourceAsync(string id)
    {
        var entity = await Context.DataSources.FindAsync(id);
        if (entity is null) return false;
        entity.LastSynced = "just now";
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisconnectSourceAsync(string id)
    {
        var entity = await Context.DataSources.FindAsync(id);
        if (entity is null) return false;
        Context.DataSources.Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }
}
