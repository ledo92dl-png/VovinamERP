using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;

namespace VovinamERP.Application.Attendance.UpdateStudentAttendance.Handlers;

public sealed class UpdateStudentAttendanceCommandHandler
    : IRequestHandler<UpdateStudentAttendanceCommand, UpdateStudentAttendanceResult>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentAttendanceCommandHandler(
        IAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateStudentAttendanceResult> Handle(
        UpdateStudentAttendanceCommand request,
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

        var updateResult = attendanceRecord.UpdateStudentAttendance(
            request.StudentId,
            request.Status,
            request.Method,
            request.Source,
            request.MarkedByUserId,
            request.IsBackfilled,
            request.Note);

        if (updateResult.IsFailure)
        {
            throw new InvalidOperationException(
                updateResult.Error.Message);
        }

        _attendanceRepository.UpdateRecord(attendanceRecord);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateStudentAttendanceResult(
            attendanceRecord.Id,
            request.StudentId,
            request.Status.ToString(),
            request.Method.ToString(),
            request.Source.ToString(),
            DateTime.UtcNow);
    }
}