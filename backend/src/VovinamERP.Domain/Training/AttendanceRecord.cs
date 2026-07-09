using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class AttendanceRecord : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid TrainingSessionId { get; private set; }
    public Guid CreatedBy { get; private set; }

    private AttendanceRecord() { }

    private AttendanceRecord(Guid tenantId, Guid trainingSessionId, Guid createdBy)
    {
        TenantId = tenantId;
        TrainingSessionId = trainingSessionId;
        CreatedBy = createdBy;

        RaiseDomainEvent(new AttendanceRecordCreatedEvent(Id, TrainingSessionId));
    }

    public static Result<AttendanceRecord> Create(Guid tenantId, Guid trainingSessionId, Guid createdBy)
    {
        if (tenantId == Guid.Empty)
            return Result<AttendanceRecord>.Failure(TrainingErrors.TenantRequired);

        if (trainingSessionId == Guid.Empty)
            return Result<AttendanceRecord>.Failure(TrainingErrors.SessionRequired);

        return Result<AttendanceRecord>.Success(new AttendanceRecord(tenantId, trainingSessionId, createdBy));
    }
}
