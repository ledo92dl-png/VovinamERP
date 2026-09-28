using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetStudentCreditHistory;

public sealed record GetStudentCreditHistoryQuery(
    Guid TenantId,
    Guid StudentId)
    : IRequest<Result<GetStudentCreditHistoryResult>>;