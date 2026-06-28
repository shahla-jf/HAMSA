using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class ChatGroupConfiguration : IEntityTypeConfiguration<ChatGroup>
{
    public void Configure(EntityTypeBuilder<ChatGroup> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(g => g.CreatedByUser)
            .WithMany()
            .HasForeignKey(g => g.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(g => g.Members)
            .WithOne(m => m.ChatGroup)
            .HasForeignKey(m => m.ChatGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(g => g.Messages)
            .WithOne(m => m.ChatGroup)
            .HasForeignKey(m => m.ChatGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatGroupMemberConfiguration : IEntityTypeConfiguration<ChatGroupMember>
{
    public void Configure(EntityTypeBuilder<ChatGroupMember> builder)
    {
        builder.HasKey(m => m.Id);

        // هر کاربر فقط یه بار می‌تونه در هر گروه عضو باشه
        builder.HasIndex(m => new { m.ChatGroupId, m.UserId })
            .IsUnique();

        builder.HasOne(m => m.User)
            .WithMany(u => u.ChatGroupMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(m => m.FileUrl)
            .HasMaxLength(500);

        builder.HasIndex(m => new { m.ChatGroupId, m.SentAt });

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
