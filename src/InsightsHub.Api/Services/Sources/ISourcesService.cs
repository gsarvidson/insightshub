using InsightsHub.Api.Models;

namespace InsightsHub.Api.Services.Sources;

public interface ISourcesService
{
    Task<(List<DataSource> Sources, List<SavedView> SavedViews)> GetSourcesAsync();
    Task<bool> SyncSourceAsync(string id);
    Task<bool> DisconnectSourceAsync(string id);
}
