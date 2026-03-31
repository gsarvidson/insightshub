namespace InsightsHub.Api.Data.Entities;

public class TagEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;

    public ICollection<OpportunityEntity> Opportunities { get; set; } = [];
    public ICollection<FeedbackItemEntity> FeedbackItems { get; set; } = [];
}
