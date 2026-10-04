using MediatR;
using VovinamERP.Application.Common.Interfaces;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.CreateManualBeltRecognition;

public sealed class CreateManualBeltRecognitionCommandHandler
    : IRequestHandler<CreateManualBeltRecognitionCommand, Result<Guid>>
{
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IRepository<BeltRankRecognition> _recognitionRepository;
    private readonly IRepository<StudentBeltHistory> _historyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateManualBeltRecognitionCommandHandler(
        IRepository<BeltRank> beltRankRepository,
        IRepository<BeltRankRecognition> recognitionRepository,
        IRepository<StudentBeltHistory> historyRepository,
        IStudentRepository studentRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
    {
        _beltRankRepository = beltRankRepository;
        _recognitionRepository = recognitionRepository;
        _historyRepository = historyRepository;
        _studentRepository = studentRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateManualBeltRecognitionCommand request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Failure(
                "BELT_RECOGNITION_MANUAL_APP_001",
                "Student was not found.");
        }

        var beltRank = await _beltRankRepository.GetByIdAsync(
            request.BeltRankId,
            cancellationToken);

        if (beltRank is null ||
            beltRank.IsArchived ||
            !beltRank.IsActive)
        {
            return Failure(
                "BELT_RECOGNITION_MANUAL_APP_002",
                "Belt rank was not found or is inactive.");
        }

        var person = await _personRepository.GetByIdAsync(
            student.PersonId,
            cancellationToken);

        if (person is null ||
            person.IsArchived ||
            person.TenantId != request.TenantId)
        {
            return Failure(
                "BELT_RECOGNITION_MANUAL_APP_003",
                "Student profile information was not found.");
        }

        var ageValidationResult = StudentBeltAgePolicy.Validate(
            person.DateOfBirth,
            beltRank,
            request.RecognitionDate);

        if (ageValidationResult.IsFailure)
        {
            return Result<Guid>.Failure(
                ageValidationResult.Error);
        }

        var recognitionResult = BeltRankRecognition.Create(
            request.TenantId,
            student.Id,
            beltRank.Id,
            request.RecognitionDate,
            BeltRankRecognitionSource.Manual,
            null,
            request.Note);

        if (recognitionResult.IsFailure ||
            recognitionResult.Value is null)
        {
            return Result<Guid>.Failure(
                recognitionResult.Error);
        }

        var laterRecognitions = await _recognitionRepository.ListAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.StudentId == student.Id &&
                !x.IsArchived &&
                x.RecognitionDate > request.RecognitionDate,
            cancellationToken);

        var laterLegacyHistories = await _historyRepository.ListAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.StudentId == student.Id &&
                !x.IsArchived &&
                x.Result == StudentBeltResult.Passed &&
                x.AwardedDate.HasValue &&
                x.AwardedDate.Value > request.RecognitionDate,
            cancellationToken);

        var hasLaterBeltMilestone =
            laterRecognitions.Count > 0 ||
            laterLegacyHistories.Count > 0;

        BeltRank? currentBeltRank = null;

        if (student.CurrentBeltRankId.HasValue)
        {
            currentBeltRank =
                await _beltRankRepository.GetByIdAsync(
                    student.CurrentBeltRankId.Value,
                    cancellationToken);
        }

        var targetIsHigherThanCurrent =
            !student.CurrentBeltRankId.HasValue ||
            (currentBeltRank is not null &&
             !currentBeltRank.IsArchived &&
             beltRank.Level > currentBeltRank.Level);

        var shouldChangeCurrentBelt =
            !hasLaterBeltMilestone &&
            targetIsHigherThanCurrent;

        if (shouldChangeCurrentBelt)
        {
            var beltChangeResult = student.ChangeCurrentBelt(
                beltRank.Id,
                request.RecognitionDate,
                request.Note,
                request.UserId);

            if (beltChangeResult.IsFailure)
            {
                return Result<Guid>.Failure(
                    beltChangeResult.Error);
            }
        }

        await _recognitionRepository.AddAsync(
            recognitionResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            recognitionResult.Value.Id);
    }

    private static Result<Guid> Failure(
        string code,
        string message)
    {
        return Result<Guid>.Failure(
            new Error(code, message));
    }
}