using MediatR;
using VovinamERP.Application.Students.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.GetStudentBeltHistory;

public sealed class GetStudentBeltHistoryQueryHandler
    : IRequestHandler<
        GetStudentBeltHistoryQuery,
        Result<GetStudentBeltHistoryResult>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentBeltHistoryRepository _beltHistoryRepository;

    public GetStudentBeltHistoryQueryHandler(
        IStudentRepository studentRepository,
        IStudentBeltHistoryRepository beltHistoryRepository)
    {
        _studentRepository = studentRepository;
        _beltHistoryRepository = beltHistoryRepository;
    }

    public async Task<Result<GetStudentBeltHistoryResult>> Handle(
        GetStudentBeltHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentBeltHistoryResult>.Failure(
                new Error(
                    "STUDENT_BELT_HISTORY_009",
                    "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentBeltHistoryResult>.Failure(
                new Error(
                    "STUDENT_BELT_HISTORY_010",
                    "Student is required."));
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<GetStudentBeltHistoryResult>.Failure(
                new Error(
                    "STUDENT_BELT_HISTORY_011",
                    "Student was not found."));
        }

        var items = await _beltHistoryRepository.ListByStudentAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        return Result<GetStudentBeltHistoryResult>.Success(
            new GetStudentBeltHistoryResult(
                request.StudentId,
                items));
    }
}