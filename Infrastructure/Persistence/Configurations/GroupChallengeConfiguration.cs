using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class GroupChallengeConfiguration : IEntityTypeConfiguration<GroupChallenge>
{
    public void Configure(EntityTypeBuilder<GroupChallenge> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .HasMaxLength(300);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(c => c.Building)
            .WithMany()
            .HasForeignKey(c => c.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Participants)
            .WithOne(p => p.GroupChallenge)
            .HasForeignKey(p => p.GroupChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GroupChallengeParticipantConfiguration : IEntityTypeConfiguration<GroupChallengeParticipant>
{
    public void Configure(EntityTypeBuilder<GroupChallengeParticipant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Gender)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.SportsBackground)
            .HasMaxLength(500);

        // هر کاربر فقط یه بار می‌تونه در هر چالش ثبت‌نام کنه
        builder.HasIndex(p => new { p.GroupChallengeId, p.UserId })
            .IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
