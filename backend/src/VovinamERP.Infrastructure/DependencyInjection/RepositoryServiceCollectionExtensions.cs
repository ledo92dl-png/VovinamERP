using Microsoft.Extensions.DependencyInjection;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.InstructorAssignments.Common;
using VovinamERP.Infrastructure.Persistence.Repositories;
using VovinamERP.Infrastructure.Repositories;

namespace VovinamERP.Infrastructure.DependencyInjection;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        // Attendance
        services.AddScoped<
            IAttendanceRepository,
            AttendanceRepository>();

        // Instructor Assignment
        services.AddScoped<
            IInstructorAssignmentRepository,
            InstructorAssignmentRepository>();

        return services;
    }
}