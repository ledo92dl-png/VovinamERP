using MediatR;
using VovinamERP.Application.Attendance.Common;
using VovinamERP.Application.Common.Interfaces;
using IPersonRepository =
    VovinamERP.Application.Common.Repositories.IPersonRepository;
using VovinamERP.Application.Students.Common;
using VovinamERP.Application.Students.QrCodes;
using VovinamERP.Domain.Students;
using VovinamERP.Domain.Training;
using VovinamERP.Domain.Belts;
using VovinamERP.SharedKernel.Results;


namespace VovinamERP.Application.Attendance.ScanStudentQr.Handlers;

public sealed class ScanStudentQrCommandHandler
    : IRequestHandler<ScanStudentQrCommand, Result<ScanStudentQrResult>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPersonRepository _personRepository;
    private readonly IRepository<BeltRank> _beltRankRepository;
    private readonly IRepository<TrainingSession> _trainingSessionRepository;
    private readonly IRepository<TrainingClass> _trainingClassRepository;

    public ScanStudentQrCommandHandler(
    IStudentRepository studentRepository,
    IPersonRepository personRepository,
    IRepository<BeltRank> beltRankRepository,
    IRepository<TrainingSession> trainingSessionRepository,
    IRepository<TrainingClass> trainingClassRepository,
    IAttendanceRepository attendanceRepository,
    IUnitOfWork unitOfWork)
{
    _studentRepository = studentRepository;
    _personRepository = personRepository;
    _beltRankRepository = beltRankRepository;
    _trainingSessionRepository = trainingSessionRepository;
    _trainingClassRepository = trainingClassRepository;
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
        var person = await _personRepository.GetByIdAsync(
    student.PersonId,
    cancellationToken);

if (person is null)
{
    return Result<ScanStudentQrResult>.Failure(
        new Error(
            "STUDENT_QR_012",
            "Student profile information was not found."));
}
        var attendanceRecord =
            await _attendanceRepository.GetRecordByIdAsync(
                request.AttendanceRecordId,
                cancellationToken);
        string? currentBeltRankName = null;

if (student.CurrentBeltRankId.HasValue)
{
    var beltRank = await _beltRankRepository.GetByIdAsync(
        student.CurrentBeltRankId.Value,
        cancellationToken);

    currentBeltRankName = beltRank?.BeltName;
}

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
        var trainingSession =
    await _trainingSessionRepository.GetByIdAsync(
        attendanceRecord.TrainingSessionId,
        cancellationToken);

if (trainingSession is null)
{
    return Result<ScanStudentQrResult>.Failure(
        new Error(
            "QR_ATTENDANCE_004",
            "Training session was not found."));
}

if (trainingSession.TenantId != request.TenantId)
{
    return Result<ScanStudentQrResult>.Failure(
        new Error(
            "QR_ATTENDANCE_005",
            "Training session does not belong to the current tenant."));
}

var trainingClass =
    await _trainingClassRepository.GetByIdAsync(
        trainingSession.TrainingClassId,
        cancellationToken);

if (trainingClass is null)
{
    return Result<ScanStudentQrResult>.Failure(
        new Error(
            "QR_ATTENDANCE_006",
            "Training class was not found."));
}

if (trainingClass.TenantId != request.TenantId)
{
    return Result<ScanStudentQrResult>.Failure(
        new Error(
            "QR_ATTENDANCE_007",
            "Training class does not belong to the current tenant."));
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
        person.FullName,
        person.AvatarUrl,
        student.CurrentBeltRankId,
        currentBeltRankName,
        trainingSession.Id,
        trainingClass.Id,
        trainingClass.Code,
        trainingClass.Name,
        trainingSession.SessionDate,
        trainingSession.StartTime,
        trainingSession.EndTime,
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
        person.FullName,
        person.AvatarUrl,
        student.CurrentBeltRankId,
        currentBeltRankName,
        trainingSession.Id,
        trainingClass.Id,
        trainingClass.Code,
        trainingClass.Name,
        trainingSession.SessionDate,
        trainingSession.StartTime,
        trainingSession.EndTime,
        QrCheckInStatus.CheckedIn,
        detail.Status,
        detail.Method,
        detail.MarkedAt,
        "Student checked in successfully by QR code."));
    }
}