using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Domain.Training;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.ScanStudentQr.Handlers;

public sealed class ScanStudentQrCommandHandler
    : IRequestHandler<ScanStudentQrCommand, Result<ScanStudentQrResult>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScanStudentQrCommandHandler(
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ScanStudentQrResult>> Handle(
        ScanStudentQrCommand request,
        CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByQrTokenAsync(
            request.TenantId,
            request.QrToken,
            cancellationToken);

        if (student is null)
        {
            return Result<ScanStudentQrResult>.Failure(
                new Error(
                    "QR_ATTENDANCE_001",
                    "QR code is invalid or the student was not found."));
        }

        var attendanceRecord =
            await _attendanceRepository.GetRecordByIdAsync(
                request.AttendanceRecordId,
                cancellationToken);

        if (attendanceRecord is null)
        {
            return Result<ScanStudentQrResult>.Failure(
                new Error(
                    "QR_ATTENDANCE_002",
                    "Attendance record was not found."));
        }

        if (attendanceRecord.TenantId != request.TenantId)
        {
            return Result<ScanStudentQrResult>.Failure(
                new Error(
                    "QR_ATTENDANCE_003",
                    "Attendance record does not belong to the current tenant."));
        }

        var markResult = attendanceRecord.MarkStudent(
            student.Id,
            AttendanceStatus.Present,
            AttendanceMethod.QrCode,
            AttendanceSource.ScheduledClass,
            request.MarkedByUserId,
            false,
            "Checked in by QR code.");

        if (markResult.IsFailure || markResult.Value is null)
        {
            return Result<ScanStudentQrResult>.Failure(
                markResult.Error);
        }

        _attendanceRepository.UpdateRecord(attendanceRecord);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<ScanStudentQrResult>.Success(
            new ScanStudentQrResult(
                attendanceRecord.Id,
                student.Id,
                student.MemberNumber,
                "Student checked in successfully by QR code."));
    }
}