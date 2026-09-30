using MediatR;
using VovinamERP.Application.Common.Interfaces;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.TransitionJuniorYellowBelt;

public sealed class TransitionJuniorYellowBeltCommandHandler
    : IRequestHandler<TransitionJuniorYellowBeltCommand, Result>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransitionJuniorYellowBeltCommandHandler(
        IStudentRepository studentRepository,
        IPersonRepository personRepository,
        IRepository<BeltRank> beltRankRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _personRepository = personRepository;
        _beltRankRepository = beltRankRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        TransitionJuniorYellowBeltCommand request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_001",
                    "Student was not found."));
        }

        if (!student.CurrentBeltRankId.HasValue)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_002",
                    "Student does not have a current belt."));
        }

        var currentBelt = await _beltRankRepository.GetByIdAsync(
            student.CurrentBeltRankId.Value,
            cancellationToken);

        if (currentBelt is null ||
            currentBelt.IsArchived ||
            !currentBelt.IsActive ||
            currentBelt.BeltCode != StudentBeltAgePolicy.JuniorYellowBeltCode)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_003",
                    "Student is not currently using the junior yellow belt."));
        }

        var person = await _personRepository.GetByIdAsync(
            student.PersonId,
            cancellationToken);

        if (person is null ||
            person.IsArchived ||
            person.TenantId != request.TenantId)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_004",
                    "Student profile information was not found."));
        }

        if (!person.DateOfBirth.HasValue)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_005",
                    "Date of birth is required for the junior yellow belt transition."));
        }

        var twelfthBirthday = person.DateOfBirth.Value.AddYears(
            StudentBeltAgePolicy.YellowBeltMinimumAge);

        if (request.TransitionDate < twelfthBirthday)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_006",
                    "Student must be at least 12 years old on the transition date."));
        }

        var yellowBelts = await _beltRankRepository.ListAsync(
            belt =>
                belt.BeltCode == StudentBeltAgePolicy.YellowBeltCode &&
                belt.IsActive &&
                !belt.IsArchived,
            cancellationToken);

        var yellowBelt = yellowBelts.SingleOrDefault();

        if (yellowBelt is null)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_007",
                    "Yellow belt was not found."));
        }

        if (yellowBelt.Level != currentBelt.Level)
        {
            return Result.Failure(
                new Error(
                    "STUDENT_BELT_TRANSITION_008",
                    "Junior yellow belt and yellow belt must have the same level."));
        }

        var changeResult = student.ChangeCurrentBelt(
            yellowBelt.Id,
            request.TransitionDate,
            request.Note,
            request.UserId);

        if (changeResult.IsFailure)
        {
            return changeResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}