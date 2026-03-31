using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsightsHub.Api.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<TeamEntity>
{
    public void Configure(EntityTypeBuilder<TeamEntity> builder)
    {
        builder.ToTable("Team");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseIdentityColumn();
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasMany(t => t.Opportunities)
            .WithMany(o => o.Teams)
            .UsingEntity(j => j.ToTable("OpportunityTeam"));

        builder.HasMany(t => t.FeedbackItems)
            .WithMany(f => f.Teams)
            .UsingEntity(j => j.ToTable("FeedbackItemTeam"));

        builder.HasData(
            new TeamEntity { Id = 1, Name = "property-b2b" },
            new TeamEntity { Id = 2, Name = "property-b2c" }
        );
    }
}
