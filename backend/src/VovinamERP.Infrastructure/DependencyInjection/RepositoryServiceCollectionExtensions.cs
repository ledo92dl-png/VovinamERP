using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Infrastructure.Persistence.Repositories;

namespace VovinamERP.Infrastructure.DependencyInjection;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }
}
