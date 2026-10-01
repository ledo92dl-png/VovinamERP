using MediatR;
using VovinamERP.Application.Common.Interfaces;
using IPersonRepository = VovinamERP.Application.Common.Repositories.IPersonRepository;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.RecordStudentBeltResult;

public sealed class RecordStudentBeltResultCommandHandler
    : IRequestHandler<RecordStudentBeltResultCommand, Result<Guid>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IRepository<StudentBeltHistory> _beltHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordStudentBeltResultCommandHandler(
        IStudentRepository studentRepository,
        IPersonRepository personRepository,
        IRepository<BeltRank> beltRankRepository,
        IRepository<StudentBeltHistory> beltHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _personRepository = personRepository;
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

        var beltRank = await _beltRankRepository.GetByIdAsync(
            request.BeltRankId,
            cancellationToken);

        if (beltRank is null ||
            beltRank.IsArchived ||
            !beltRank.IsActive)
        {
            return Result<Guid>.Failure(
                new Error(
                    "STUDENT_BELT_002",
                    "Belt rank was not found or is inactive."));
        }

        if (request.Result == StudentBeltResult.Passed &&
            request.AwardedDate is null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "STUDENT_BELT_004",
                    "Awarded date is required for a passed belt result."));
        }

        if (request.Result == StudentBeltResult.Passed)
        {
            var person = await _personRepository.GetByIdAsync(
                student.PersonId,
                cancellationToken);

            if (person is null ||
                person.IsArchived ||
                person.TenantId != request.TenantId)
            {
                return Result<Guid>.Failure(
                    new Error(
                        "STUDENT_BELT_003",
                        "Student profile information was not found."));
            }

            var ageValidationResult = StudentBeltAgePolicy.Validate(
                person.DateOfBirth,
                beltRank,
                request.AwardedDate!.Value);

            if (ageValidationResult.IsFailure)
            {
                return Result<Guid>.Failure(
                    ageValidationResult.Error);
            }
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
                return Result<Guid>.Failure(
                    beltChangeResult.Error);
            }
        }

        await _beltHistoryRepository.AddAsync(
            history,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(history.Id);
    }
}