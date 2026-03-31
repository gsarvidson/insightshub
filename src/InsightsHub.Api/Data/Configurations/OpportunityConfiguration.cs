using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsightsHub.Api.Data.Configurations;

public class OpportunityConfiguration : IEntityTypeConfiguration<OpportunityEntity>
{
    public void Configure(EntityTypeBuilder<OpportunityEntity> builder)
    {
        builder.ToTable("Opportunity");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
    }
}
