using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.BeltExams;
using VovinamERP.Domain.Belts;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.CreateBeltExam;

public sealed class CreateBeltExamCommandHandler
    : IRequestHandler<CreateBeltExamCommand, Result<Guid>>
{
    private readonly IRepository<BeltExam> _beltExamRepository;
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBeltExamCommandHandler(
        IRepository<BeltExam> beltExamRepository,
        IRepository<BeltRank> beltRankRepository,
        IUnitOfWork unitOfWork)
    {
        _beltExamRepository = beltExamRepository;
        _beltRankRepository = beltRankRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateBeltExamCommand request,
        CancellationToken cancellationToken)
    {
        var targetBeltRank = await _beltRankRepository.GetByIdAsync(
            request.TargetBeltRankId,
            cancellationToken);

        if (targetBeltRank is null ||
            targetBeltRank.IsArchived ||
            !targetBeltRank.IsActive)
        {
            return Result<Guid>.Failure(
                new Error(
                    "BELT_EXAM_APP_001",
                    "Target belt rank was not found or is inactive."));
        }

        var createResult = BeltExam.Create(
            request.TenantId,
            request.TargetBeltRankId,
            request.ExamDate,
            request.SessionName,
            request.Location,
            request.SourceBeltName,
            request.Note,
            request.UserId);

        if (createResult.IsFailure ||
            createResult.Value is null)
        {
            return Result<Guid>.Failure(
                createResult.Error);
        }

        var beltExam = createResult.Value;

        await _beltExamRepository.AddAsync(
            beltExam,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            beltExam.Id);
    }
}