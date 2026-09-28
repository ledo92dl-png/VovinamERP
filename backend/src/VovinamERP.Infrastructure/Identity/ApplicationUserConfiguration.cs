using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VovinamERP.Infrastructure.Identity;

public sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasIndex(x => x.TenantId);

        builder.HasIndex(x => new
        {
            x.TenantId,
            x.UserName
        });

        builder.HasIndex(x => x.PersonId);
    }
}