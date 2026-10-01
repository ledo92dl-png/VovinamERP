using MediatR;
using VovinamERP.Application.BeltRecognitions.Common;
using VovinamERP.Application.Students.Common;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.GetStudentBeltRecognitions;

public sealed class GetStudentBeltRecognitionsQueryHandler
    : IRequestHandler<
        GetStudentBeltRecognitionsQuery,
        Result<GetStudentBeltRecognitionsResult>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IBeltRankRecognitionRepository _recognitionRepository;

    public GetStudentBeltRecognitionsQueryHandler(
        IStudentRepository studentRepository,
        IBeltRankRecognitionRepository recognitionRepository)
    {
        _studentRepository = studentRepository;
        _recognitionRepository = recognitionRepository;
    }

    public async Task<Result<GetStudentBeltRecognitionsResult>> Handle(
        GetStudentBeltRecognitionsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId == Guid.Empty)
        {
            return Result<GetStudentBeltRecognitionsResult>.Failure(
                new Error(
                    "BELT_RECOGNITION_QUERY_001",
                    "Tenant is required."));
        }

        if (request.StudentId == Guid.Empty)
        {
            return Result<GetStudentBeltRecognitionsResult>.Failure(
                new Error(
                    "BELT_RECOGNITION_QUERY_002",
                    "Student is required."));
        }

        var student = await _studentRepository.GetByIdAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        if (student is null)
        {
            return Result<GetStudentBeltRecognitionsResult>.Failure(
                new Error(
                    "BELT_RECOGNITION_QUERY_003",
                    "Student was not found."));
        }

        var items = await _recognitionRepository.ListByStudentAsync(
            request.TenantId,
            request.StudentId,
            cancellationToken);

        return Result<GetStudentBeltRecognitionsResult>.Success(
            new GetStudentBeltRecognitionsResult(
                request.StudentId,
                items));
    }
}