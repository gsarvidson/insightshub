namespace InsightsHub.Api.Data.Entities;

public class TeamEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<OpportunityEntity> Opportunities { get; set; } = [];
    public ICollection<FeedbackItemEntity> FeedbackItems { get; set; } = [];
}
