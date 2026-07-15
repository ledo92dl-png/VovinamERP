namespace VovinamERP.Api.Contracts.Attendance;

public sealed record ScanStudentQrRequest(
    Guid TenantId,
    string QrToken,
    Guid MarkedByUserId);