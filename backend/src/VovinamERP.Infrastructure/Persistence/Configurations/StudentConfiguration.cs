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
        builder.Property(x => x.MartialName).HasMaxLength(128);
        builder.Property(x => x.IntroducedBy).HasMaxLength(256);
        builder.Property(x => x.MartialProfileNote).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.MemberNumber }).IsUnique();
        builder.HasIndex(x => x.PersonId);
        builder.HasIndex(x => x.OrganizationId);
    }
}
