using VovinamERP.SharedKernel.Common;

namespace VovinamERP.Domain.Training;

public sealed record AttendanceDetailUpdatedEvent(
    Guid AttendanceDetailId,
    Guid AttendanceRecordId,
    Guid StudentId,
    AttendanceStatus Status,
    AttendanceMethod Method,
    AttendanceSource Source
) : DomainEvent;