using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.InstructorAssignments.Common;
using VovinamERP.Domain.InstructorAssignments;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class InstructorAssignmentRepository : IInstructorAssignmentRepository
{
    private readonly VovinamDbContext _context;

    public InstructorAssignmentRepository(VovinamDbContext context)
    {
        _context = context;
    }

    public async Task<InstructorAssignment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<InstructorAssignment>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<InstructorAssignment>> GetByTrainingClassIdAsync(
        Guid trainingClassId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<InstructorAssignment>()
            .Where(x => x.TrainingClassId == trainingClassId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        InstructorAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<InstructorAssignment>()
            .AddAsync(assignment, cancellationToken);
    }

    public void Update(InstructorAssignment assignment)
    {
        _context.Set<InstructorAssignment>()
            .Update(assignment);
    }
}