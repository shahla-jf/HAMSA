using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.PostalCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(b => b.ImageUrl)
            .HasMaxLength(500);

        builder.Property(b => b.SharedElectricityCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.SharedWaterCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.CleaningCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.ElevatorCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.LatePenaltyPercent)
            .HasColumnType("decimal(5,2)");

        // روابط
        builder.HasMany(b => b.Units)
            .WithOne(u => u.Building)
            .HasForeignKey(u => u.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Memberships)
            .WithOne(m => m.Building)
            .HasForeignKey(m => m.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.ManagerHistory)
            .WithOne(h => h.Building)
            .HasForeignKey(h => h.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Announcements)
            .WithOne(a => a.Building)
            .HasForeignKey(a => a.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.ChatGroups)
            .WithOne(g => g.Building)
            .HasForeignKey(g => g.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
