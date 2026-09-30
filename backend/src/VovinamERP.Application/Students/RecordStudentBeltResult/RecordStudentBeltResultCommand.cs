using MediatR;
using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.RecordStudentBeltResult;

public sealed record RecordStudentBeltResultCommand(
    Guid TenantId,
    Guid StudentId,
    Guid BeltRankId,
    DateOnly ExamDate,
    StudentBeltResult Result,
    DateOnly? AwardedDate,
    string? Note,
    Guid? UserId
) : IRequest<Result<Guid>>;
