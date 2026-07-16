using MediatR;
using VovinamERP.SharedKernel.Results;

namespace VovinamERP.Application.Attendance.ScanStudentQr;

public sealed record ScanStudentQrCommand(
    Guid TenantId,
    Guid AttendanceRecordId,
    string QrContent,
    Guid MarkedByUserId)
    : IRequest<Result<ScanStudentQrResult>>;