using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FacilityType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // یه امکان در یه روز فقط یه بار می‌تونه رزرو بشه
        builder.HasIndex(r => new { r.BuildingId, r.FacilityType, r.Date })
            .IsUnique();

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Building)
            .WithMany()
            .HasForeignKey(r => r.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
