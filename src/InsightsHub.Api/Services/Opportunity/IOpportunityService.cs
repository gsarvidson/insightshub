using InsightsHub.Api.Models;
using OpportunityModel = InsightsHub.Api.Models.Opportunity;

namespace InsightsHub.Api.Services.Opportunity;

public interface IOpportunityService
{
    Task<List<OpportunityModel>> GetOpportunitiesAsync(string? statusFilter);
    Task<OpportunityModel?> GetOpportunityAsync(string id);
    Task<bool> UpdateStatusAsync(string id, string status);
    Task<OpportunityModel> CreateOpportunityAsync(CreateOpportunityRequest req);
    Task RefreshAiNotesAsync(string opportunityId);
}
