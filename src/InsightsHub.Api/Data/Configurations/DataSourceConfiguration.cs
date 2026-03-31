using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsightsHub.Api.Data.Configurations;

public class DataSourceConfiguration : IEntityTypeConfiguration<DataSourceEntity>
{
    public void Configure(EntityTypeBuilder<DataSourceEntity> builder)
    {
        builder.ToTable("DataSource");
        builder.HasKey(d => d.Id);

        builder.HasMany(d => d.Opportunities)
            .WithMany(o => o.DataSources)
            .UsingEntity(j => j.ToTable("OpportunityDataSource"));

        builder.HasMany(d => d.FeedbackItems)
            .WithMany(f => f.DataSources)
            .UsingEntity(j => j.ToTable("FeedbackItemDataSource"));
    }
}

public class SavedViewConfiguration : IEntityTypeConfiguration<SavedViewEntity>
{
    public void Configure(EntityTypeBuilder<SavedViewEntity> builder)
    {
        builder.ToTable("SavedView");
        builder.HasKey(s => s.Id);
    }
}
