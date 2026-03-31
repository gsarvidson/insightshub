using InsightsHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsightsHub.Api.Data.Configurations;

public class FeedbackItemConfiguration : IEntityTypeConfiguration<FeedbackItemEntity>
{
    public void Configure(EntityTypeBuilder<FeedbackItemEntity> builder)
    {
        builder.ToTable("FeedbackItem");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(f => f.IsAiProcessed).HasDefaultValue(false);

        builder.HasOne(f => f.Opportunity)
            .WithMany()
            .HasForeignKey(f => f.OpportunityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
