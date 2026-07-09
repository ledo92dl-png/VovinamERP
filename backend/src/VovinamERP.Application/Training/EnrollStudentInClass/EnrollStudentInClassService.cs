using VovinamERP.Domain.Training;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Training.EnrollStudentInClass;

public sealed class EnrollStudentInClassService
{
    public Result<EnrollStudentInClassDraft> Prepare(EnrollStudentInClassCommand command)
    {
        var enrollmentResult = StudentClassEnrollment.Create(
            command.TenantId,
            command.StudentId,
            command.TrainingClassId,
            command.StartDate,
            command.Status,
            command.Note);

        if (enrollmentResult.IsFailure || enrollmentResult.Value is null)
            return Result<EnrollStudentInClassDraft>.Failure(enrollmentResult.Error);

        var response = new EnrollStudentInClassResponse(
            enrollmentResult.Value.Id,
            enrollmentResult.Value.StudentId,
            enrollmentResult.Value.TrainingClassId,
            enrollmentResult.Value.StartDate,
            enrollmentResult.Value.Status);

        return Result<EnrollStudentInClassDraft>.Success(
            new EnrollStudentInClassDraft(enrollmentResult.Value, response));
    }
}
