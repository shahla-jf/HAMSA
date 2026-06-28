using HAMSA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HAMSA.Infrastructure.Persistence.Configurations;

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.PhoneNumber)
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(o => o.Code)
            .HasMaxLength(5)
            .IsRequired();

        builder.HasIndex(o => new { o.PhoneNumber, o.IsUsed });
    }
}