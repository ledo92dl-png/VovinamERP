using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.GetBeltExams;

public sealed record GetBeltExamsQuery(
    Guid TenantId)
    : IRequest<Result<IReadOnlyList<BeltExamListItem>>>;