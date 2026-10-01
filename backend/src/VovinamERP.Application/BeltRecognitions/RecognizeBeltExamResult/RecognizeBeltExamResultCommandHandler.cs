using MediatR;
using VovinamERP.Application.Common.Interfaces;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.RecordStudentBeltResult;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.BeltRecognitions;
using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.RecognizeBeltExamResult;

public sealed class RecognizeBeltExamResultCommandHandler
    : IRequestHandler<RecognizeBeltExamResultCommand, Result<Guid>>
{
    private readonly IRepository<BeltExamStudentResult> _resultRepository;
    private readonly IRepository<BeltExam> _examRepository;
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IRepository<BeltRankRecognition> _recognitionRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecognizeBeltExamResultCommandHandler(
        IRepository<BeltExamStudentResult> resultRepository,
        IRepository<BeltExam> examRepository,
        IRepository<BeltRank> beltRankRepository,
        IRepository<BeltRankRecognition> recognitionRepository,
        IStudentRepository studentRepository,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
    {
        _resultRepository = resultRepository;
        _examRepository = examRepository;
        _beltRankRepository = beltRankRepository;
        _recognitionRepository = recognitionRepository;
        _studentRepository = studentRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        RecognizeBeltExamResultCommand request,
        CancellationToken cancellationToken)
    {
        var examResult = await _resultRepository.GetByIdAsync(
            request.BeltExamStudentResultId,
            cancellationToken);

        if (examResult is null ||
            examResult.IsArchived ||
            examResult.TenantId != request.TenantId)
        {
            return Failure(
                "BELT_RECOGNITION_APP_001",
                "Belt exam student result was not found.");
        }

        if (examResult.Result != StudentBeltResult.Passed)
        {
            return Failure(
                "BELT_RECOGNITION_APP_002",
                "Only a passed belt exam result can be recognized.");
        }

        var exam = await _examRepository.GetByIdAsync(
            examResult.BeltExamId,
            cancellationToken);

        if (exam is null ||
            exam.IsArchived ||
            exam.TenantId != request.TenantId)
        {
            return Failure(
                "BELT_RECOGNITION_APP_003",
                "Belt exam was not found.");
        }

        if (request.RecognitionDate < exam.ExamDate)
        {
            return Failure(
                "BELT_RECOGNITION_APP_004",
                "Recognition date cannot be earlier than exam date.");
        }

        var beltRank = await _beltRankRepository.GetByIdAsync(
            exam.TargetBeltRankId,
            cancellationToken);

        if (beltRank is null ||
            beltRank.IsArchived ||
            !beltRank.IsActive)
        {
            return Failure(
                "BELT_RECOGNITION_APP_005",
                "Target belt rank was not found or is inactive.");
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            examResult.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Failure(
                "BELT_RECOGNITION_APP_006",
                "Student was not found.");
        }

        var alreadyRecognized = await _recognitionRepository.ExistsAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.BeltExamStudentResultId ==
                    request.BeltExamStudentResultId,
            cancellationToken);

        if (alreadyRecognized)
        {
            return Failure(
                "BELT_RECOGNITION_APP_007",
                "This belt exam result has already been recognized.");
        }

        var person = await _personRepository.GetByIdAsync(
            student.PersonId,
            cancellationToken);

        if (person is null ||
            person.IsArchived ||
            person.TenantId != request.TenantId)
        {
            return Failure(
                "BELT_RECOGNITION_APP_008",
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
            BeltRankRecognitionSource.BeltExam,
            examResult.Id,
            request.Note);

        if (recognitionResult.IsFailure ||
            recognitionResult.Value is null)
        {
            return Result<Guid>.Failure(
                recognitionResult.Error);
        }

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
