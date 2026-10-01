using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.GetBeltExamScoreSheet;

public sealed record GetBeltExamScoreSheetQuery(
    Guid TenantId,
    Guid BeltExamId,
    Guid BeltExamStudentResultId)
    : IRequest<Result<BeltExamScoreSheetResult>>;
