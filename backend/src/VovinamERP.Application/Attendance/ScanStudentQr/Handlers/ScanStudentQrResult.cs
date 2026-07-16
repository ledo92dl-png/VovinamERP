using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.ScanStudentQr;

public sealed record ScanStudentQrResult(
    Guid AttendanceRecordId,
    Guid StudentId,
    string MemberNumber,
    QrCheckInStatus CheckInStatus,
    AttendanceStatus AttendanceStatus,
    AttendanceMethod Method,
    DateTimeOffset MarkedAt,
    string Message);