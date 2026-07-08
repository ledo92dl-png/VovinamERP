using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Students;

public sealed record StudentCreatedEvent(
    Guid StudentId,
    Guid PersonId,
    string MemberNumber
) : DomainEvent;

public sealed record StudentUpdatedEvent(Guid StudentId) : DomainEvent;

public sealed record StudentStatusChangedEvent(
    Guid StudentId,
    StudentStatus Status
) : DomainEvent;

public sealed record StudentArchivedEvent(Guid StudentId) : DomainEvent;
