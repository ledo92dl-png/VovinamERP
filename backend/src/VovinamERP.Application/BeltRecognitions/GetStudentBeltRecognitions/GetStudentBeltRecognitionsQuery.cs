using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltRecognitions.GetStudentBeltRecognitions;

public sealed record GetStudentBeltRecognitionsQuery(
    Guid TenantId,
    Guid StudentId)
    : IRequest<Result<GetStudentBeltRecognitionsResult>>;