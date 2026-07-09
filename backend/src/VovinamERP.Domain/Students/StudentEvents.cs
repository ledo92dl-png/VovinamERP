using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Students;

public sealed record StudentRegisteredEvent(
    Guid StudentId,
    Guid TenantId,
    Guid PersonId,
    string MemberNumber
) : DomainEvent;

public sealed record StudentMartialProfileUpdatedEvent(
    Guid StudentId
) : DomainEvent;

public sealed record StudentStatusChangedEvent(
    Guid StudentId,
    StudentStatus OldStatus,
    StudentStatus NewStatus,
    string? Reason
) : DomainEvent;

public sealed record StudentBeltChangedEvent(
    Guid StudentId,
    Guid BeltRankId,
    DateOnly AwardedDate,
    string? Note
) : DomainEvent;

public sealed record StudentArchivedEvent(
    Guid StudentId
) : DomainEvent;
