using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.BeltExams;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.AddBeltExamStudentResult;

public sealed class AddBeltExamStudentResultCommandHandler
    : IRequestHandler<AddBeltExamStudentResultCommand, Result<Guid>>
{
    private readonly IRepository<BeltExam> _beltExamRepository;
    private readonly IRepository<BeltExamStudentResult> _resultRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBeltExamStudentResultCommandHandler(
        IRepository<BeltExam> beltExamRepository,
        IRepository<BeltExamStudentResult> resultRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork)
    {
        _beltExamRepository = beltExamRepository;
        _resultRepository = resultRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddBeltExamStudentResultCommand request,
        CancellationToken cancellationToken)
    {
        var exam = await _beltExamRepository.GetByIdAsync(
            request.BeltExamId,
            cancellationToken);

        if (exam is null ||
            exam.IsArchived ||
            exam.TenantId != request.TenantId)
        {
            return Result<Guid>.Failure(
                AddBeltExamStudentResultErrors.BeltExamNotFound);
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<Guid>.Failure(
                AddBeltExamStudentResultErrors.StudentNotFound);
        }

        var duplicateExists = await _resultRepository.ExistsAsync(
            x =>
                x.TenantId == request.TenantId &&
                x.BeltExamId == request.BeltExamId &&
                x.StudentId == request.StudentId,
            cancellationToken);

        if (duplicateExists)
        {
            return Result<Guid>.Failure(
                AddBeltExamStudentResultErrors.StudentAlreadyAdded);
        }

        var createResult = BeltExamStudentResult.Create(
            request.TenantId,
            request.BeltExamId,
            request.StudentId,
            request.UnitName,
            request.SourceResult,
            request.Result,
            request.TotalScore,
            request.Ranking,
            request.Note,
            request.UserId);

        if (createResult.IsFailure)
            return Result<Guid>.Failure(createResult.Error);

        var beltExamStudentResult = createResult.Value;

        await _resultRepository.AddAsync(
            beltExamStudentResult,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(beltExamStudentResult.Id);
    }
}

public static class AddBeltExamStudentResultErrors
{
    public static readonly Error BeltExamNotFound =
        new(
            "BELT_EXAM_RESULT_APP_001",
            "Belt exam was not found for the specified tenant.");

    public static readonly Error StudentNotFound =
        new(
            "BELT_EXAM_RESULT_APP_002",
            "Student was not found for the specified tenant.");

    public static readonly Error StudentAlreadyAdded =
        new(
            "BELT_EXAM_RESULT_APP_003",
            "Student already has a result in this belt exam.");
}
