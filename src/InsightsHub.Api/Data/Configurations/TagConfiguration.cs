using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsightsHub.Api.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.ToTable("Tag");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityColumn();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Opportunities)
            .WithMany(o => o.Tags)
            .UsingEntity(j => j.ToTable("OpportunityTag"));

        builder.HasMany(t => t.FeedbackItems)
            .WithMany(f => f.Tags)
            .UsingEntity(j => j.ToTable("FeedbackItemTag"));
    }
}
