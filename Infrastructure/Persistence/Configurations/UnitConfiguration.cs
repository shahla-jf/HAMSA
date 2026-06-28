using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Block).IsRequired();
        builder.Property(u => u.Floor).IsRequired();
        builder.Property(u => u.UnitNumber).IsRequired();

        // یه ساختمان نمی‌تونه دو واحد با بلوک+طبقه+شماره یکسان داشته باشه
        builder.HasIndex(u => new { u.BuildingId, u.Block, u.Floor, u.UnitNumber })
            .IsUnique();

        builder.HasMany(u => u.Charges)
            .WithOne(c => c.Unit)
            .HasForeignKey(c => c.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BuildingMembershipConfiguration : IEntityTypeConfiguration<BuildingMembership>
{
    public void Configure(EntityTypeBuilder<BuildingMembership> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.InviteCode)
            .HasMaxLength(20);

        builder.HasIndex(m => m.InviteCode)
            .IsUnique()
            .HasFilter("[InviteCode] IS NOT NULL");  // برای SQL Server

        builder.HasOne(m => m.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Unit)
            .WithMany(u => u.Memberships)
            .HasForeignKey(m => m.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BuildingManagerHistoryConfiguration : IEntityTypeConfiguration<BuildingManagerHistory>
{
    public void Configure(EntityTypeBuilder<BuildingManagerHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
