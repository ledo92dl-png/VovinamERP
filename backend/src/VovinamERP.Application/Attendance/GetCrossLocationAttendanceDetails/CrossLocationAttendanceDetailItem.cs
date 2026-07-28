using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;

public sealed record CrossLocationAttendanceDetailItem(
    Guid AttendanceDetailId,
    Guid StudentId,
    string MemberNumber,
    string FullName,

    Guid HomeOrganizationId,
    string HomeOrganizationName,

    Guid TrainingOrganizationId,
    string TrainingOrganizationName,

    Guid TrainingClassId,
    string TrainingClassCode,
    string TrainingClassName,

    Guid TrainingSessionId,
    DateOnly SessionDate,
    TimeOnly StartTime,
    TimeOnly EndTime,

    AttendanceStatus AttendanceStatus,
    AttendanceMethod Method,
    DateTimeOffset MarkedAt,
    string? Note);