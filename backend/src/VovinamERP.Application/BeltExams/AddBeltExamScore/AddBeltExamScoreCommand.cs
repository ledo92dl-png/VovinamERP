using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.AddBeltExamScore;

public sealed record AddBeltExamScoreCommand(
    Guid TenantId,
    Guid BeltExamStudentResultId,
    Guid BeltExamSubjectId,
    decimal Score,
    Guid? UserId)
    : IRequest<Result<Guid>>;
