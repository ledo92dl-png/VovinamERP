using MediatR;
using VovinamERP.Application.Attendance.Common;

namespace VovinamERP.Application.Attendance.GetAttendanceRecords.Handlers;

public sealed class GetAttendanceRecordsQueryHandler
    : IRequestHandler<GetAttendanceRecordsQuery, GetAttendanceRecordsResult>
{
    private readonly IAttendanceRepository _attendanceRepository;

    public GetAttendanceRecordsQueryHandler(
        IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<GetAttendanceRecordsResult> Handle(
        GetAttendanceRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var records = await _attendanceRepository.GetPagedAsync(
            request.TenantId,
            request.TrainingSessionId,
            skip,
            request.PageSize,
            cancellationToken);

        var totalCount = await _attendanceRepository.CountAsync(
            request.TenantId,
            request.TrainingSessionId,
            cancellationToken);

        var items = records
            .Select(record => new AttendanceRecordListItem(
                record.Id,
                record.TenantId,
                record.TrainingSessionId,
                record.CreatedByUserId,
                record.Details.Count))
            .ToList();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetAttendanceRecordsResult(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages);
    }
}