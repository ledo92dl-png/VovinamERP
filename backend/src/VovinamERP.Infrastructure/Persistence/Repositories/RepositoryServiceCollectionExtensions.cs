using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Application.Common.Repositories;

namespace VovinamERP.Infrastructure.Persistence.Repositories;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddVovinamRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();

        return services;
    }
}
