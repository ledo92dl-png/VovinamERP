using MediatR;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.RecordStudentBeltResult;

public sealed class RecordStudentBeltResultCommandHandler
    : IRequestHandler<RecordStudentBeltResultCommand, Result<Guid>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IRepository<StudentBeltHistory> _beltHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordStudentBeltResultCommandHandler(
        IStudentRepository studentRepository,
        IRepository<BeltRank> beltRankRepository,
        IRepository<StudentBeltHistory> beltHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _beltRankRepository = beltRankRepository;
        _beltHistoryRepository = beltHistoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        RecordStudentBeltResultCommand request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "STUDENT_BELT_001",
                    "Student was not found."));
        }

        var beltRankExists = await _beltRankRepository.ExistsAsync(
            beltRank =>
                beltRank.Id == request.BeltRankId &&
                !beltRank.IsArchived &&
                beltRank.IsActive,
            cancellationToken);

        if (!beltRankExists)
        {
            return Result<Guid>.Failure(
                new Error(
                    "STUDENT_BELT_002",
                    "Belt rank was not found or is inactive."));
        }

        var historyResult = StudentBeltHistory.Create(
            request.TenantId,
            student.Id,
            request.BeltRankId,
            request.ExamDate,
            request.Result,
            request.AwardedDate,
            request.Note);

        if (historyResult.IsFailure || historyResult.Value is null)
        {
            return Result<Guid>.Failure(historyResult.Error);
        }

        var history = historyResult.Value;

        if (request.Result == StudentBeltResult.Passed)
        {
            var beltChangeResult = student.ChangeCurrentBelt(
                request.BeltRankId,
                request.AwardedDate!.Value,
                request.Note,
                request.UserId);

            if (beltChangeResult.IsFailure)
            {
                return Result<Guid>.Failure(beltChangeResult.Error);
            }
        }

        await _beltHistoryRepository.AddAsync(
            history,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(history.Id);
    }
}
