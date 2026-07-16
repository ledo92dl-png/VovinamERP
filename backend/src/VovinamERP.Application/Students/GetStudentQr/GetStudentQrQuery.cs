using MediatR;

namespace VovinamERP.Application.Students.GetStudentQr;

public sealed record GetStudentQrQuery(
    Guid TenantId,
    Guid StudentId)
    : IRequest<GetStudentQrResult>;