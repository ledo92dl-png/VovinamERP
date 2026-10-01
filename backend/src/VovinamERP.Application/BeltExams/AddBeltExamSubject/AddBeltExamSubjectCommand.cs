using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.AddBeltExamSubject;

public sealed record AddBeltExamSubjectCommand(
    Guid TenantId,
    Guid BeltExamId,
    string Name,
    int DisplayOrder,
    decimal? MaximumScore,
    Guid? UserId)
    : IRequest<Result<Guid>>;