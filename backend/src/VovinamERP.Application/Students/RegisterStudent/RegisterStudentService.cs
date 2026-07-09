using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.RegisterStudent;

public sealed class RegisterStudentService
{
    public Result<RegisterStudentDraft> Prepare(
        RegisterStudentCommand command,
        IStudentRegistrationCodeGenerator codeGenerator)
    {
        ArgumentNullException.ThrowIfNull(codeGenerator);

        var personCode = codeGenerator.NewPersonCode();
        var memberNumber = codeGenerator.NewMemberNumber();

        var personResult = Person.Create(
            command.TenantId,
            personCode,
            command.FullName,
            command.Gender,
            command.DateOfBirth,
            command.PhoneNumber,
            command.Email,
            command.Address,
            command.AvatarUrl);

        if (personResult.IsFailure || personResult.Value is null)
            return Result<RegisterStudentDraft>.Failure(personResult.Error);

        var studentResult = Student.Create(
            command.TenantId,
            personResult.Value.Id,
            command.OrganizationId,
            memberNumber,
            command.EnrollmentDate,
            command.CurrentBeltRankId,
            command.MartialName,
            command.IntroducedBy,
            command.MartialProfileNote);

        if (studentResult.IsFailure || studentResult.Value is null)
            return Result<RegisterStudentDraft>.Failure(studentResult.Error);

        var response = new RegisterStudentResponse(
            personResult.Value.Id,
            studentResult.Value.Id,
            personResult.Value.Code,
            studentResult.Value.MemberNumber,
            personResult.Value.FullName,
            studentResult.Value.EnrollmentDate);

        return Result<RegisterStudentDraft>.Success(
            new RegisterStudentDraft(personResult.Value, studentResult.Value, response));
    }
}
