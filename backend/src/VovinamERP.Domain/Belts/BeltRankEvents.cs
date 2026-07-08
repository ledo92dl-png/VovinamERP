using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Belts;

public sealed record BeltRankCreatedEvent(
    Guid BeltRankId,
    string BeltCode,
    string BeltName
) : DomainEvent;

public sealed record BeltRankUpdatedEvent(
    Guid BeltRankId
) : DomainEvent;

public sealed record BeltRankActivatedEvent(
    Guid BeltRankId
) : DomainEvent;

public sealed record BeltRankDeactivatedEvent(
    Guid BeltRankId
) : DomainEvent;

public sealed record BeltRankArchivedEvent(
    Guid BeltRankId
) : DomainEvent;