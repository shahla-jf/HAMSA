using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(l => l.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(l => l.ContactPhone)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(l => l.ImageUrl)
            .HasMaxLength(500);

        builder.HasIndex(l => new { l.BuildingId, l.IsSold });

        builder.HasOne(l => l.CreatedByUser)
            .WithMany()
            .HasForeignKey(l => l.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Building)
            .WithMany()
            .HasForeignKey(l => l.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ResidentEventConfiguration : IEntityTypeConfiguration<ResidentEvent>
{
    public void Configure(EntityTypeBuilder<ResidentEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrganizerFullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(e => e.ContactPhone)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(e => e.RegistrationFee)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Building)
            .WithMany()
            .HasForeignKey(e => e.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Sessions)
            .WithOne(s => s.ResidentEvent)
            .HasForeignKey(s => s.ResidentEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Registrations)
            .WithOne(r => r.ResidentEvent)
            .HasForeignKey(r => r.ResidentEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EventSessionConfiguration : IEntityTypeConfiguration<EventSession>
{
    public void Configure(EntityTypeBuilder<EventSession> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.StartTime).IsRequired();
        builder.Property(s => s.EndTime).IsRequired();
    }
}

public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
{
    public void Configure(EntityTypeBuilder<EventRegistration> builder)
    {
        builder.HasKey(r => r.Id);

        // هر کاربر فقط یه بار می‌تونه در هر رویداد ثبت‌نام کنه
        builder.HasIndex(r => new { r.ResidentEventId, r.UserId })
            .IsUnique();

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
