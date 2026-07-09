using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Training;

public sealed class StudentClassEnrollment : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid TrainingClassId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public string? Note { get; private set; }

    private StudentClassEnrollment() { }

    private StudentClassEnrollment(
        Guid tenantId,
        Guid studentId,
        Guid trainingClassId,
        DateOnly startDate,
        string? note)
    {
        TenantId = tenantId;
        StudentId = studentId;
        TrainingClassId = trainingClassId;
        StartDate = startDate;
        Note = note?.Trim();
        Status = EnrollmentStatus.Active;

        RaiseDomainEvent(new StudentEnrolledInClassEvent(Id, TrainingClassId, StudentId));
    }

    public static Result<StudentClassEnrollment> Create(
        Guid tenantId,
        Guid studentId,
        Guid trainingClassId,
        DateOnly startDate,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.TenantRequired);

        if (studentId == Guid.Empty)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.StudentRequired);

        if (trainingClassId == Guid.Empty)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.ClassRequired);

        if (startDate == default)
            return Result<StudentClassEnrollment>.Failure(TrainingErrors.InvalidDate);

        return Result<StudentClassEnrollment>.Success(new StudentClassEnrollment(tenantId, studentId, trainingClassId, startDate, note));
    }

    public Result EndEnrollment(DateOnly endDate, EnrollmentStatus status, string? note)
    {
        if (IsArchived)
            return Result.Failure(TrainingErrors.AlreadyArchived);

        if (endDate < StartDate)
            return Result.Failure(TrainingErrors.InvalidDate);

        EndDate = endDate;
        Status = status;
        Note = note?.Trim();
        MarkUpdated(null);

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        Status = EnrollmentStatus.Archived;
        base.Archive(userId);
    }
}
