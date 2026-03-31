namespace InsightsHub.Api.Data.Entities;

public class FeedbackItemEntity
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Sentiment { get; set; }
    public ICollection<TagEntity> Tags { get; set; } = [];
    public ICollection<TeamEntity> Teams { get; set; } = [];
    public ICollection<DataSourceEntity> DataSources { get; set; } = [];
    public string? OpportunityId { get; set; }
    public OpportunityEntity? Opportunity { get; set; }
    public string UserType { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string AiNote { get; set; } = string.Empty;
    public bool IsAiProcessed { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
