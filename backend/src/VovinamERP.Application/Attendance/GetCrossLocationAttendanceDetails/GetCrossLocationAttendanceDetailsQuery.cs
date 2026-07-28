using MediatR;

namespace VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;

public sealed record GetCrossLocationAttendanceDetailsQuery(
    Guid TenantId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    Guid? StudentId,
    Guid? TrainingOrganizationId,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = "sessionDate",
    bool Descending = true)
    : IRequest<GetCrossLocationAttendanceDetailsResult>;