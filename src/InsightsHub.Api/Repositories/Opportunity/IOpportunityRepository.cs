using InsightsHub.Api.Data.Entities;

namespace InsightsHub.Api.Repositories.Opportunity;

public interface IOpportunityRepository
{
    Task<List<OpportunityEntity>> GetAllAsync();
    Task<OpportunityEntity?> GetByIdAsync(string id);
    Task<bool> UpdateStatusAsync(string id, string status);
    Task<OpportunityEntity> CreateAsync(OpportunityEntity entity, IEnumerable<string>? tagNames = null);
    Task<int> CountAsync();
    Task<bool> UpdateAiNotesAsync(string id, string aiNotes);
    Task<List<string>> GetFeedbackTextsAsync(string opportunityId);
    Task<Dictionary<string, List<(string Name, int Count)>>> GetSourceCountsByOpportunityAsync();
    Task<Dictionary<string, List<DateTime>>> GetFeedbackDatesByOpportunityAsync();
}
