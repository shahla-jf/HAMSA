using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class RepairReportConfiguration : IEntityTypeConfiguration<RepairReport>
{
    public void Configure(EntityTypeBuilder<RepairReport> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(r => r.Priority)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(r => new { r.BuildingId, r.Status });

        builder.HasOne(r => r.ReportedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.MediaFiles)
            .WithOne(m => m.RepairReport)
            .HasForeignKey(m => m.RepairReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RepairReportMediaConfiguration : IEntityTypeConfiguration<RepairReportMedia>
{
    public void Configure(EntityTypeBuilder<RepairReportMedia> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Url)
            .HasMaxLength(500)
            .IsRequired();
    }
}
