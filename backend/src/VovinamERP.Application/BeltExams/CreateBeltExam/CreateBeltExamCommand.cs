using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.CreateBeltExam;

public sealed record CreateBeltExamCommand(
    Guid TenantId,
    Guid TargetBeltRankId,
    DateOnly ExamDate,
    string SessionName,
    string Location,
    string SourceBeltName,
    string? Note,
    Guid? UserId)
    : IRequest<Result<Guid>>;