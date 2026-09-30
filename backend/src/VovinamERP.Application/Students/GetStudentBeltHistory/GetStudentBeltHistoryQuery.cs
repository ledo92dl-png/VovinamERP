using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.GetStudentBeltHistory;

public sealed record GetStudentBeltHistoryQuery(
    Guid TenantId,
    Guid StudentId
) : IRequest<Result<GetStudentBeltHistoryResult>>;