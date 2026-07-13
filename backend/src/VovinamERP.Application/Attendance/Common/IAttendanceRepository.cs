using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.Common;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetRecordByIdAsync(
        Guid attendanceRecordId,
        CancellationToken cancellationToken = default);

    Task<AttendanceRecord?> GetByIdAsync(
        Guid attendanceRecordId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceRecord>> GetPagedAsync(
        Guid tenantId,
        Guid? trainingSessionId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid tenantId,
        Guid? trainingSessionId,
        CancellationToken cancellationToken = default);

    Task AddRecordAsync(
        AttendanceRecord attendanceRecord,
        CancellationToken cancellationToken = default);

    void UpdateRecord(AttendanceRecord attendanceRecord);
}