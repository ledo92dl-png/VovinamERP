using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.QrCodes;
using VovinamERP.Domain.Students;
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
        var payloadResult = StudentQrPayloadParser.Parse(
            request.QrContent);

        if (payloadResult.IsFailure || payloadResult.Value is null)
        {
            return Result<ScanStudentQrResult>.Failure(
                payloadResult.Error);
        }

        var payload = payloadResult.Value;

        if (payload.TenantId != request.TenantId)
        {
            return Result<ScanStudentQrResult>.Failure(
                new Error(
                    "STUDENT_QR_008",
                    "QR code belongs to another tenant."));
        }

        var student = await _studentRepository.GetByQrTokenAsync(
            request.TenantId,
            payload.QrToken,
            cancellationToken);

        if (student is null)
        {
            return Result<ScanStudentQrResult>.Failure(
                new Error(
                    "STUDENT_QR_009",
                    "Student was not found."));
        }

        if (student.IsArchived)
        {
            return Result<ScanStudentQrResult>.Failure(
                new Error(
                    "STUDENT_QR_010",
                    "Student has been archived."));
        }

        if (student.Status != StudentStatus.Active)
        {
            return Result<ScanStudentQrResult>.Failure(
                new Error(
                    "STUDENT_QR_011",
                    "Student is not active."));
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

        var existingDetail = attendanceRecord.Details
            .FirstOrDefault(
                x => x.StudentId == student.Id &&
                     !x.IsArchived);

        if (existingDetail is not null)
        {
            return Result<ScanStudentQrResult>.Success(
                new ScanStudentQrResult(
                    attendanceRecord.Id,
                    student.Id,
                    student.MemberNumber,
                    QrCheckInStatus.AlreadyCheckedIn,
                    existingDetail.Status,
                    existingDetail.Method,
                    existingDetail.MarkedAt,
                    "Student has already been checked in."));
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

        var detail = markResult.Value;

        return Result<ScanStudentQrResult>.Success(
            new ScanStudentQrResult(
                attendanceRecord.Id,
                student.Id,
                student.MemberNumber,
                QrCheckInStatus.CheckedIn,
                detail.Status,
                detail.Method,
                detail.MarkedAt,
                "Student checked in successfully by QR code."));
    }
}