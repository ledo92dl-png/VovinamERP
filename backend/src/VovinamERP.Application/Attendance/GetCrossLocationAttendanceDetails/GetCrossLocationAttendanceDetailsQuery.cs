using MediatR;

namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;

public sealed record GetCrossLocationAttendanceDetailsQuery(
    Guid TenantId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    Guid? StudentId,
    Guid? TrainingOrganizationId)
    : IRequest<GetCrossLocationAttendanceDetailsResult>;