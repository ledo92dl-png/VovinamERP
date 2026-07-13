using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;

namespace VovinamERP.Application.Attendance.MarkStudentAttendance.Handlers;

public sealed class MarkStudentAttendanceCommandHandler
    : IRequestHandler<MarkStudentAttendanceCommand, MarkStudentAttendanceResult>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkStudentAttendanceCommandHandler(
        IAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MarkStudentAttendanceResult> Handle(
        MarkStudentAttendanceCommand request,
        CancellationToken cancellationToken)
    {
        var attendanceRecord =
            await _attendanceRepository.GetRecordByIdAsync(
                request.AttendanceRecordId,
                cancellationToken);

        if (attendanceRecord is null)
        {
            throw new InvalidOperationException(
                "Attendance record was not found.");
        }

        if (attendanceRecord.TenantId != request.TenantId)
        {
            throw new InvalidOperationException(
                "Attendance record does not belong to the current tenant.");
        }

        var detailResult = attendanceRecord.MarkStudent(
            request.StudentId,
            request.Status,
            request.Method,
            request.Source,
            request.MarkedByUserId,
            request.IsBackfilled,
            request.Note);

        if (detailResult.IsFailure || detailResult.Value is null)
        {
            throw new InvalidOperationException(
                detailResult.Error.Message);
        }

        _attendanceRepository.UpdateRecord(attendanceRecord);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detail = detailResult.Value;

        return new MarkStudentAttendanceResult(
            attendanceRecord.Id,
            detail.Id,
            detail.StudentId,
            detail.Status,
            detail.Method,
            detail.Source,
            detail.MarkedAt,
            detail.IsBackfilled);
    }
}