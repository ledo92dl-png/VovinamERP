using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.AddBeltExamSubject;

public sealed class AddBeltExamSubjectCommandHandler
    : IRequestHandler<AddBeltExamSubjectCommand, Result<Guid>>
{
    private readonly IRepository<BeltExam> _beltExamRepository;
    private readonly IRepository<BeltExamSubject> _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddBeltExamSubjectCommandHandler(
        IRepository<BeltExam> beltExamRepository,
        IRepository<BeltExamSubject> subjectRepository,
        IUnitOfWork unitOfWork)
    {
        _beltExamRepository = beltExamRepository;
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddBeltExamSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var beltExam = await _beltExamRepository.GetByIdAsync(
            request.BeltExamId,
            cancellationToken);

        if (beltExam is null ||
            beltExam.IsArchived ||
            beltExam.TenantId != request.TenantId)
        {
            return Result<Guid>.Failure(
                new Error(
                    "BELT_EXAM_SUBJECT_APP_001",
                    "Belt exam was not found for the specified tenant."));
        }

        var createResult = BeltExamSubject.Create(
            request.TenantId,
            request.BeltExamId,
            request.Name,
            request.DisplayOrder,
            request.MaximumScore,
            request.UserId);

        if (createResult.IsFailure ||
            createResult.Value is null)
        {
            return Result<Guid>.Failure(
                createResult.Error);
        }

        var subject = createResult.Value;

        var duplicateDisplayOrder =
            await _subjectRepository.ExistsAsync(
                x =>
                    x.TenantId == request.TenantId &&
                    x.BeltExamId == request.BeltExamId &&
                    !x.IsArchived &&
                    x.DisplayOrder == subject.DisplayOrder,
                cancellationToken);

        if (duplicateDisplayOrder)
        {
            return Result<Guid>.Failure(
                new Error(
                    "BELT_EXAM_SUBJECT_APP_002",
                    "Display order already exists in this belt exam."));
        }

        var duplicateName =
            await _subjectRepository.ExistsAsync(
                x =>
                    x.TenantId == request.TenantId &&
                    x.BeltExamId == request.BeltExamId &&
                    !x.IsArchived &&
                    x.Name.ToLower() == subject.Name.ToLower(),
                cancellationToken);

        if (duplicateName)
        {
            return Result<Guid>.Failure(
                new Error(
                    "BELT_EXAM_SUBJECT_APP_003",
                    "Subject name already exists in this belt exam."));
        }

        await _subjectRepository.AddAsync(
            subject,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(subject.Id);
    }
}