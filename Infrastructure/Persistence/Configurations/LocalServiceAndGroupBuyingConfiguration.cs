using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class LocalServiceConfiguration : IEntityTypeConfiguration<LocalService>
{
    public void Configure(EntityTypeBuilder<LocalService> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Category)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(s => s.ProviderName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.ContactPhone)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(s => s.WorkingHours)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(s => new { s.BuildingId, s.Category });

        builder.HasOne(s => s.CreatedByUser)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Building)
            .WithMany()
            .HasForeignKey(s => s.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Ratings)
            .WithOne(r => r.LocalService)
            .HasForeignKey(r => r.LocalServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LocalServiceRatingConfiguration : IEntityTypeConfiguration<LocalServiceRating>
{
    public void Configure(EntityTypeBuilder<LocalServiceRating> builder)
    {
        builder.HasKey(r => r.Id);

        // هر کاربر فقط یه بار می‌تونه به هر خدمت امتیاز بده
        builder.HasIndex(r => new { r.LocalServiceId, r.UserId })
            .IsUnique();

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class GroupBuyingConfiguration : IEntityTypeConfiguration<GroupBuying>
{
    public void Configure(EntityTypeBuilder<GroupBuying> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.OrganizerFullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(g => g.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(g => g.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(g => g.ContactPhone)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(g => g.ImageUrl)
            .HasMaxLength(500);

        builder.HasIndex(g => new { g.BuildingId, g.Deadline });

        builder.HasOne(g => g.CreatedByUser)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.Building)
            .WithMany()
            .HasForeignKey(g => g.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Participants)
            .WithOne(p => p.GroupBuying)
            .HasForeignKey(p => p.GroupBuyingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GroupBuyingParticipantConfiguration : IEntityTypeConfiguration<GroupBuyingParticipant>
{
    public void Configure(EntityTypeBuilder<GroupBuyingParticipant> builder)
    {
        builder.HasKey(p => p.Id);

        // هر کاربر فقط یه بار می‌تونه به هر کمپین بپیونده
        builder.HasIndex(p => new { p.GroupBuyingId, p.UserId })
            .IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
