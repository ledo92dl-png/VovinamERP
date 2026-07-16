using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.ScanStudentQr;

public sealed record ScanStudentQrResult(
    Guid AttendanceRecordId,
    Guid StudentId,
    string MemberNumber,
    string FullName,
    string? AvatarUrl,
    Guid? CurrentBeltRankId,
    string? CurrentBeltRankName,

    Guid TrainingSessionId,
    Guid TrainingClassId,
    string TrainingClassCode,
    string TrainingClassName,

    Guid TrainingOrganizationId,
    string TrainingOrganizationName,
    string? TrainingOrganizationAddress,

    Guid StudentHomeOrganizationId,
    string StudentHomeOrganizationName,
    bool IsCrossLocation,

    DateOnly SessionDate,
    TimeOnly StartTime,
    TimeOnly EndTime,

    QrCheckInStatus CheckInStatus,
    AttendanceStatus AttendanceStatus,
    AttendanceMethod Method,
    DateTimeOffset MarkedAt,
    string Message);