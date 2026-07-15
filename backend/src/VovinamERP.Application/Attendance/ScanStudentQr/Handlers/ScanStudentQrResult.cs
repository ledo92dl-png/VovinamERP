namespace VovinamERP.Application.Attendance.ScanStudentQr;

public sealed record ScanStudentQrResult(
    Guid AttendanceRecordId,
    Guid StudentId,
    string MemberNumber,
    string Message);