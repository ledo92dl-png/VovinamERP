namespace VovinamERP.Application.Students.QrCodes;

public sealed record StudentQrPayload(
    Guid TenantId,
    string QrToken);