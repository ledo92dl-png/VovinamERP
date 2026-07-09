using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VovinamERP.Domain.Persons;

namespace VovinamERP.Infrastructure.Persistence.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Address).HasMaxLength(512);
        builder.Property(x => x.AvatarUrl).HasMaxLength(1024);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
