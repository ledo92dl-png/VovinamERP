namespace VovinamERP.Api.Contracts.Attendance;

public sealed record CreateAttendanceRecordRequest(
    Guid TenantId,
    Guid TrainingSessionId,
    Guid CreatedByUserId);