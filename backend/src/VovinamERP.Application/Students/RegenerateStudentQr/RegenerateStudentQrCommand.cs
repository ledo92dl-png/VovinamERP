using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Students.RegenerateStudentQr;

public sealed record RegenerateStudentQrCommand(
    Guid TenantId,
    Guid StudentId,
    Guid RegeneratedByUserId)
    : IRequest<Result<RegenerateStudentQrResult>>;