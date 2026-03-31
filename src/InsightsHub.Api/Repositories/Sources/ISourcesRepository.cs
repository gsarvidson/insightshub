using InsightsHub.Api.Data.Entities;

namespace InsightsHub.Api.Repositories.Sources;

public interface ISourcesRepository
{
    Task<List<DataSourceEntity>> GetAllSourcesAsync();
    Task<List<SavedViewEntity>> GetAllSavedViewsAsync();
    Task<bool> SyncSourceAsync(string id);
    Task<bool> DisconnectSourceAsync(string id);
}
