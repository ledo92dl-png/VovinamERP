using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class AttendanceRecord : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TrainingSessionId { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private AttendanceRecord() { }

    private AttendanceRecord(Guid tenantId, Guid trainingSessionId, Guid createdByUserId)
    {
        TenantId = tenantId;
        TrainingSessionId = trainingSessionId;
        CreatedByUserId = createdByUserId;

        RaiseDomainEvent(new AttendanceRecordCreatedEvent(Id, TrainingSessionId));
    }

    public static Result<AttendanceRecord> Create(Guid tenantId, Guid trainingSessionId, Guid createdByUserId)
    {
        if (tenantId == Guid.Empty)
            return Result<AttendanceRecord>.Failure(TrainingErrors.TenantRequired);

        if (trainingSessionId == Guid.Empty)
            return Result<AttendanceRecord>.Failure(TrainingErrors.SessionRequired);

        return Result<AttendanceRecord>.Success(new AttendanceRecord(tenantId, trainingSessionId, createdByUserId));
    }
}
