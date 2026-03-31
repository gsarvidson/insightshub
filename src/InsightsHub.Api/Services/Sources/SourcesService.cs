using InsightsHub.Api.Models;
using InsightsHub.Api.Repositories.Sources;

namespace InsightsHub.Api.Services.Sources;

public class SourcesService(ISourcesRepository repository) : ISourcesService
{
    public async Task<(List<DataSource> Sources, List<SavedView> SavedViews)> GetSourcesAsync()
    {
        var sourceEntities = await repository.GetAllSourcesAsync();
        var savedViewEntities = await repository.GetAllSavedViewsAsync();

        var sources = sourceEntities
            .Select(s => new DataSource(s.Id, s.Name, s.LastSynced, s.Description, s.Status))
            .ToList();

        var savedViews = savedViewEntities
            .Select(v => new SavedView(v.Name, v.Meta))
            .ToList();

        return (sources, savedViews);
    }

    public Task<bool> SyncSourceAsync(string id) =>
        repository.SyncSourceAsync(id);

    public Task<bool> DisconnectSourceAsync(string id) =>
        repository.DisconnectSourceAsync(id);
}
