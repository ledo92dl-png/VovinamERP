using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Training;

public sealed record AttendanceRecordCompletedEvent(
    Guid AttendanceRecordId,
    Guid TrainingSessionId,
    Guid CompletedByUserId,
    DateTimeOffset CompletedAt
) : DomainEvent;