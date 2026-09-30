using VovinamERP.SharedKernel.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Domain.Students;

public sealed class StudentBeltHistory : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid BeltRankId { get; private set; }

    public DateOnly ExamDate { get; private set; }
    public StudentBeltResult Result { get; private set; }
    public DateOnly? AwardedDate { get; private set; }
    public string? Note { get; private set; }

    private StudentBeltHistory()
    {
    }

    private StudentBeltHistory(
        Guid tenantId,
        Guid studentId,
        Guid beltRankId,
        DateOnly examDate,
        StudentBeltResult result,
        DateOnly? awardedDate,
        string? note)
    {
        TenantId = tenantId;
        StudentId = studentId;
        BeltRankId = beltRankId;
        ExamDate = examDate;
        Result = result;
        AwardedDate = awardedDate;
        Note = note?.Trim();
    }

    public static Result<StudentBeltHistory> Create(
        Guid tenantId,
        Guid studentId,
        Guid beltRankId,
        DateOnly examDate,
        StudentBeltResult result,
        DateOnly? awardedDate,
        string? note)
    {
        if (tenantId == Guid.Empty)
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.TenantRequired);

        if (studentId == Guid.Empty)
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.StudentRequired);

        if (beltRankId == Guid.Empty)
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.BeltRankRequired);

        if (examDate == default)
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.ExamDateRequired);

        if (!Enum.IsDefined(result))
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.InvalidResult);

        if (result == StudentBeltResult.Passed && awardedDate is null)
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.AwardedDateRequired);

        if (result == StudentBeltResult.Failed && awardedDate is not null)
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.AwardedDateNotAllowed);

        if (awardedDate.HasValue && awardedDate.Value < examDate)
            return Result<StudentBeltHistory>.Failure(
                StudentBeltHistoryErrors.AwardedDateBeforeExam);

        return Result<StudentBeltHistory>.Success(
            new StudentBeltHistory(
                tenantId,
                studentId,
                beltRankId,
                examDate,
                result,
                awardedDate,
                note));
    }
}
