using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.CompleteAttendanceRecord.Handlers;

public sealed class CompleteAttendanceRecordCommandHandler
    : IRequestHandler<
        CompleteAttendanceRecordCommand,
        Result<CompleteAttendanceRecordResult>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteAttendanceRecordCommandHandler(
        IAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CompleteAttendanceRecordResult>> Handle(
        CompleteAttendanceRecordCommand request,
        CancellationToken cancellationToken)
    {
        var attendanceRecord =
            await _attendanceRepository.GetRecordByIdAsync(
                request.AttendanceRecordId,
                cancellationToken);

        if (attendanceRecord is null)
        {
            return Result<CompleteAttendanceRecordResult>.Failure(
                new Error(
                    "TRAINING_023",
                    "Attendance record was not found."));
        }

        if (attendanceRecord.TenantId != request.TenantId)
        {
            return Result<CompleteAttendanceRecordResult>.Failure(
                new Error(
                    "TRAINING_024",
                    "Attendance record does not belong to the current tenant."));
        }

        var completeResult = attendanceRecord.Complete(
            request.CompletedByUserId);

        if (completeResult.IsFailure)
        {
            return Result<CompleteAttendanceRecordResult>.Failure(
                completeResult.Error);
        }

        _attendanceRepository.UpdateRecord(attendanceRecord);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CompleteAttendanceRecordResult>.Success(
            new CompleteAttendanceRecordResult(
                attendanceRecord.Id,
                attendanceRecord.TrainingSessionId,
                attendanceRecord.Status,
                attendanceRecord.CompletedAt!.Value,
                attendanceRecord.CompletedByUserId!.Value));
    }
}