using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class AttendanceDetail : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid AttendanceRecordId { get; private set; }
    public Guid StudentId { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public string? Note { get; private set; }

    private AttendanceDetail() { }

    private AttendanceDetail(
        Guid tenantId,
        Guid attendanceRecordId,
        Guid studentId,
        AttendanceStatus status,
        string? note)
    {
        TenantId = tenantId;
        AttendanceRecordId = attendanceRecordId;
        StudentId = studentId;
        Status = status;
        Note = note?.Trim();

        RaiseDomainEvent(new AttendanceDetailMarkedEvent(Id, AttendanceRecordId, StudentId, Status));
    }

    public static Result<AttendanceDetail> Create(
        Guid tenantId,
        Guid attendanceRecordId,
        Guid studentId,
        AttendanceStatus status,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<AttendanceDetail>.Failure(TrainingErrors.TenantRequired);

        if (attendanceRecordId == Guid.Empty)
            return Result<AttendanceDetail>.Failure(TrainingErrors.AttendanceRecordRequired);

        if (studentId == Guid.Empty)
            return Result<AttendanceDetail>.Failure(TrainingErrors.StudentRequired);

        return Result<AttendanceDetail>.Success(new AttendanceDetail(tenantId, attendanceRecordId, studentId, status, note));
    }

    public Result UpdateStatus(AttendanceStatus status, string? note)
    {
        if (IsArchived)
            return Result.Failure(TrainingErrors.AlreadyArchived);

        Status = status;
        Note = note?.Trim();
        MarkUpdated(null);
        RaiseDomainEvent(new AttendanceDetailMarkedEvent(Id, AttendanceRecordId, StudentId, Status));

        return Result.Success();
    }
}
