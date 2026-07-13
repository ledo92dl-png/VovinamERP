using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.Common;

public interface IAttendanceRepository
{
    Task<AttendanceRecord?> GetRecordByIdAsync(
        Guid attendanceRecordId,
        CancellationToken cancellationToken = default);

    Task AddRecordAsync(
        AttendanceRecord attendanceRecord,
        CancellationToken cancellationToken = default);

    void UpdateRecord(AttendanceRecord attendanceRecord);
}