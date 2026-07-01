using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class MonthlyChargeRateConfiguration : IEntityTypeConfiguration<MonthlyChargeRate>
{
    public void Configure(EntityTypeBuilder<MonthlyChargeRate> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // یه ساختمان فقط یه نرخ شارژ برای هر ماه داره
        builder.HasIndex(r => new { r.BuildingId, r.Year, r.Month })
            .IsUnique();

        builder.HasOne(r => r.Building)
            .WithMany()
            .HasForeignKey(r => r.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
