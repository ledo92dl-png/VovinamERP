using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Instructors;

public sealed record InstructorCreatedEvent(
    Guid InstructorId,
    Guid TenantId,
    Guid PersonId,
    string InstructorNumber
) : DomainEvent;

public sealed record InstructorArchivedEvent(
    Guid InstructorId
) : DomainEvent;
