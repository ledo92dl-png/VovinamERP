using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Clubs;

public sealed record ClubCreatedEvent(
    Guid ClubId,
    string ClubCode,
    string ClubName
) : DomainEvent;

public sealed record ClubUpdatedEvent(
    Guid ClubId
) : DomainEvent;

public sealed record ClubArchivedEvent(
    Guid ClubId
) : DomainEvent;
