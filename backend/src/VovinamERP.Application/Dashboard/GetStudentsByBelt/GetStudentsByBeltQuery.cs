using MediatR;

namespace VovinamERP.Application.Dashboard.GetStudentsByBelt;

public sealed record GetStudentsByBeltQuery(
    Guid TenantId,
    Guid? OrganizationId)
    : IRequest<GetStudentsByBeltResult>;