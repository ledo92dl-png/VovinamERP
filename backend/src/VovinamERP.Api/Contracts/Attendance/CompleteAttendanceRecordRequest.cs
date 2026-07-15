namespace VovinamERP.Api.Contracts.Attendance;

public sealed record CompleteAttendanceRecordRequest(
    Guid TenantId,
    Guid CompletedByUserId);