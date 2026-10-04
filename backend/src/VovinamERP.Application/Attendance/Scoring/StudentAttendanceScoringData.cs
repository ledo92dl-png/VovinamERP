using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.Scoring;

public sealed record EligibleAttendanceSessionItem(
    Guid TrainingSessionId,
    DateOnly SessionDate);

public sealed record StudentAttendanceSessionItem(
    Guid TrainingSessionId,
    DateOnly SessionDate,
    AttendanceStatus Status,
    bool IsCrossLocation);

public sealed record StudentAttendanceScoringData(
    IReadOnlyList<EligibleAttendanceSessionItem> EligibleSessions,
    IReadOnlyList<StudentAttendanceSessionItem> AttendanceSessions);
