using HAMSA.Domain.Entities;
using HAMSA.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.PenaltyAmount)
            .HasColumnType("decimal(18,2)");

        // یه واحد نمی‌تونه دو شارژ برای یه ماه داشته باشه
        builder.HasIndex(c => new { c.UnitId, c.Year, c.Month })
            .IsUnique();

        builder.HasOne(c => c.Building)
            .WithMany()
            .HasForeignKey(c => c.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Charge)
            .HasForeignKey(t => t.ChargeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.TrackingCode)
            .HasMaxLength(50);

        builder.HasIndex(t => t.TrackingCode)
            .IsUnique()
            .HasFilter("[TrackingCode] IS NOT NULL");

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



public class BuildingExpenseConfiguration : IEntityTypeConfiguration<BuildingExpense>
{
    public void Configure(EntityTypeBuilder<BuildingExpense> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(e => e.Building)
            .WithMany()
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        // اضافه کردن رابطه با کاربر ثبت کننده
        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}