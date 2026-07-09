using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Persons;

public sealed record PersonCreatedEvent(
    Guid PersonId,
    Guid TenantId,
    string Code,
    string FullName
) : DomainEvent;

public sealed record PersonUpdatedEvent(
    Guid PersonId
) : DomainEvent;

public sealed record PersonArchivedEvent(
    Guid PersonId
) : DomainEvent;
