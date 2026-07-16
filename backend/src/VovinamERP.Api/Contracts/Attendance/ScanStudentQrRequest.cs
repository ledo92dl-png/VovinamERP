namespace VovinamERP.Api.Contracts.Attendance;

public sealed record ScanStudentQrRequest(
    Guid TenantId,
    string QrContent,
    Guid MarkedByUserId);