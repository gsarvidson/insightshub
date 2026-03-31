namespace InsightsHub.Api.Data.Entities;

public class DataSourceEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LastSynced { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<OpportunityEntity> Opportunities { get; set; } = [];
    public ICollection<FeedbackItemEntity> FeedbackItems { get; set; } = [];
}
