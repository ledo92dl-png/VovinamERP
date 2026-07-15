using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.SharedKernel.Results;
using VovinamERP.Domain.Training;

namespace VovinamERP.Application.Attendance.CreateAttendanceRecord.Handlers;

public sealed class CreateAttendanceRecordCommandHandler
    : IRequestHandler<CreateAttendanceRecordCommand, Result<CreateAttendanceRecordResult>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAttendanceRecordCommandHandler(
        IAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateAttendanceRecordResult>> Handle(
        CreateAttendanceRecordCommand request,
        CancellationToken cancellationToken)
    {
        var exists =
            await _attendanceRepository.ExistsForTrainingSessionAsync(
                request.TenantId,
                request.TrainingSessionId,
                cancellationToken);

        if (exists)
        {
            return Result<CreateAttendanceRecordResult>.Failure(
                new Error(
                    "TRAINING_020",
                    "Attendance record already exists for this training session."));
        }

        var attendanceResult =
            AttendanceRecord.Create(
                request.TenantId,
                request.TrainingSessionId,
                request.CreatedByUserId);

        if (attendanceResult.IsFailure || attendanceResult.Value is null)
        {
            return Result<CreateAttendanceRecordResult>.Failure(
                attendanceResult.Error);
        }

        await _attendanceRepository.AddRecordAsync(
            attendanceResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

       return Result<CreateAttendanceRecordResult>.Success(
    new CreateAttendanceRecordResult(
        attendanceResult.Value.Id,
        attendanceResult.Value.TenantId,
        attendanceResult.Value.TrainingSessionId,
        attendanceResult.Value.CreatedByUserId));
    }
}