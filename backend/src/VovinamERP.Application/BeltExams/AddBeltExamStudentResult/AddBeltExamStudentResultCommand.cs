using MediatR;
using VovinamERP.Domain.Students;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.BeltExams.AddBeltExamStudentResult;

public sealed record AddBeltExamStudentResultCommand(
    Guid TenantId,
    Guid BeltExamId,
    Guid StudentId,
    string UnitName,
    string SourceResult,
    StudentBeltResult Result,
    decimal? TotalScore,
    int? Ranking,
    string? Note,
    Guid? UserId)
    : IRequest<Result<Guid>>;