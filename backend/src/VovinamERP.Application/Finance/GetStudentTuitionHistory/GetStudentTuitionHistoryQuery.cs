using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Finance.GetStudentTuitionHistory;

public sealed record GetStudentTuitionHistoryQuery(
    Guid TenantId,
    Guid StudentId,
    int Months = 6)
    : IRequest<Result<GetStudentTuitionHistoryResult>>;