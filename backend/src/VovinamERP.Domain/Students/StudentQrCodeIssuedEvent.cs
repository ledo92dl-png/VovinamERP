using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Students;

public sealed record StudentQrCodeIssuedEvent(
    Guid StudentId,
    Guid TenantId,
    string QrToken
) : DomainEvent;