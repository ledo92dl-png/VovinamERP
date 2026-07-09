using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Guardians;

public sealed class StudentGuardian : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid GuardianId { get; private set; }
    public StudentGuardianRelationship Relationship { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? Note { get; private set; }

    private StudentGuardian()
    {
    }

    private StudentGuardian(
        Guid tenantId,
        Guid studentId,
        Guid guardianId,
        StudentGuardianRelationship relationship,
        bool isPrimary,
        string? note)
    {
        TenantId = tenantId;
        StudentId = studentId;
        GuardianId = guardianId;
        Relationship = relationship;
        IsPrimary = isPrimary;
        Note = note?.Trim();
    }

    public static Result<StudentGuardian> Create(
        Guid tenantId,
        Guid studentId,
        Guid guardianId,
        StudentGuardianRelationship relationship,
        bool isPrimary,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<StudentGuardian>.Failure(StudentGuardianErrors.TenantRequired);

        if (studentId == Guid.Empty)
            return Result<StudentGuardian>.Failure(StudentGuardianErrors.StudentRequired);

        if (guardianId == Guid.Empty)
            return Result<StudentGuardian>.Failure(StudentGuardianErrors.GuardianRequired);

        return Result<StudentGuardian>.Success(
            new StudentGuardian(tenantId, studentId, guardianId, relationship, isPrimary, note));
    }

    public Result Update(
        StudentGuardianRelationship relationship,
        bool isPrimary,
        string? note,
        Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(StudentGuardianErrors.AlreadyArchived);

        Relationship = relationship;
        IsPrimary = isPrimary;
        Note = note?.Trim();
        MarkUpdated(userId);

        return Result.Success();
    }

    public Result SetPrimary(bool isPrimary, Guid? userId)
    {
        if (IsArchived)
            return Result.Failure(StudentGuardianErrors.AlreadyArchived);

        IsPrimary = isPrimary;
        MarkUpdated(userId);

        return Result.Success();
    }

    public override void Archive(Guid? userId)
    {
        if (IsArchived) return;

        base.Archive(userId);
    }
}
