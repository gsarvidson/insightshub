using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsightsHub.Api.Data;

public class InsightsHubDbContext(DbContextOptions<InsightsHubDbContext> options) : DbContext(options)
{
    public DbSet<OpportunityEntity> Opportunities { get; set; }
    public DbSet<FeedbackItemEntity> FeedbackItems { get; set; }
    public DbSet<DataSourceEntity> DataSources { get; set; }
    public DbSet<SavedViewEntity> SavedViews { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<TeamEntity> Teams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsightsHubDbContext).Assembly);
    }
}
