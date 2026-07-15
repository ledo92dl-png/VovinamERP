using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Students;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MemberNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.QrToken)
    .HasMaxLength(64)
    .IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.MartialName).HasMaxLength(128);
        builder.Property(x => x.IntroducedBy).HasMaxLength(256);
        builder.Property(x => x.MartialProfileNote).HasMaxLength(2048);
        builder.HasIndex(x => new { x.TenantId, x.MemberNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.QrToken })
    .IsUnique();
        builder.HasIndex(x => x.PersonId).IsUnique();
        builder.HasIndex(x => x.OrganizationId);
        builder.HasIndex(x => x.CurrentBeltRankId);
    }
}
