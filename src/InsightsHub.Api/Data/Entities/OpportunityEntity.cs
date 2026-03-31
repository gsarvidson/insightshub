namespace InsightsHub.Api.Data.Entities;

public class OpportunityEntity
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Sub { get; set; } = string.Empty;
    public ICollection<TagEntity> Tags { get; set; } = [];
    public ICollection<TeamEntity> Teams { get; set; } = [];
    public ICollection<DataSourceEntity> DataSources { get; set; } = [];
    public string Color { get; set; } = string.Empty;
    public string? AiNotes { get; set; }
    public DateTime CreatedAt { get; set; }

}
