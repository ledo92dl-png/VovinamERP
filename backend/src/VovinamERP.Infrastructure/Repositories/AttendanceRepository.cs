using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Domain.Training;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Infrastructure.Repositories;

public sealed class AttendanceRepository : IAttendanceRepository
{
    private readonly VovinamDbContext _context;

    public AttendanceRepository(VovinamDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceRecord?> GetRecordByIdAsync(
        Guid attendanceRecordId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AttendanceRecord>()
            .Include(x => x.Details)
            .FirstOrDefaultAsync(
                x => x.Id == attendanceRecordId,
                cancellationToken);
    }

    public async Task AddRecordAsync(
        AttendanceRecord attendanceRecord,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<AttendanceRecord>()
            .AddAsync(attendanceRecord, cancellationToken);
    }

    public void UpdateRecord(AttendanceRecord attendanceRecord)
    {
        _context.Set<AttendanceRecord>()
            .Update(attendanceRecord);
    }
}