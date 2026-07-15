using VovinamERP.Application.Attendance.ScanStudentQr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Contracts.Attendance;
using VovinamERP.Application.Attendance.CreateAttendanceRecord;
using VovinamERP.Application.Attendance.GetAttendanceRecord;
using VovinamERP.Application.Attendance.GetAttendanceRecords;
using VovinamERP.Application.Attendance.MarkStudentAttendance;
using VovinamERP.Application.Attendance.UpdateStudentAttendance;
using VovinamERP.Application.Attendance.CompleteAttendanceRecord;
namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/attendance-records")]
public sealed class AttendanceController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
public async Task<IActionResult> CreateAttendanceRecord(
    CreateAttendanceRecordRequest request,
    CancellationToken cancellationToken)
{
    var command = new CreateAttendanceRecordCommand(
        request.TenantId,
        request.TrainingSessionId,
        request.CreatedByUserId);

    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure || result.Value is null)
    {
        return BadRequest(new
        {
            Code = result.Error.Code,
            Message = result.Error.Message
        });
    }

    var value = result.Value;

    return CreatedAtAction(
        nameof(GetAttendanceRecord),
        new
        {
            attendanceRecordId = value.AttendanceRecordId,
            tenantId = value.TenantId
        },
        value);
}

    [HttpGet]
    public async Task<IActionResult> GetAttendanceRecords(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid? trainingSessionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAttendanceRecordsQuery(
            tenantId,
            trainingSessionId,
            pageNumber,
            pageSize);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{attendanceRecordId:guid}")]
    public async Task<IActionResult> GetAttendanceRecord(
        Guid attendanceRecordId,
        [FromQuery] Guid tenantId,
        CancellationToken cancellationToken)
    {
        var query = new GetAttendanceRecordQuery(
            attendanceRecordId,
            tenantId);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("{attendanceRecordId:guid}/students")]
    public async Task<IActionResult> MarkStudentAttendance(
        Guid attendanceRecordId,
        MarkStudentAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MarkStudentAttendanceCommand(
            request.TenantId,
            attendanceRecordId,
            request.StudentId,
            request.Status,
            request.Method,
            request.Source,
            request.MarkedByUserId,
            request.IsBackfilled,
            request.Note);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{attendanceRecordId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> UpdateStudentAttendance(
        Guid attendanceRecordId,
        Guid studentId,
        UpdateStudentAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateStudentAttendanceCommand(
            attendanceRecordId,
            studentId,
            request.Status,
            request.Method,
            request.Source,
            request.MarkedByUserId,
            request.IsBackfilled,
            request.Note);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("{attendanceRecordId:guid}/complete")]
public async Task<IActionResult> CompleteAttendanceRecord(
    Guid attendanceRecordId,
    CompleteAttendanceRecordRequest request,
    CancellationToken cancellationToken)
{
    var command = new CompleteAttendanceRecordCommand(
        request.TenantId,
        attendanceRecordId,
        request.CompletedByUserId);

    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure || result.Value is null)
    {
        return BadRequest(new
        {
            Code = result.Error.Code,
            Message = result.Error.Message
        });
    }

    return Ok(result.Value);
}
[HttpPost("{attendanceRecordId:guid}/scan-qr")]
public async Task<IActionResult> ScanStudentQr(
    Guid attendanceRecordId,
    ScanStudentQrRequest request,
    CancellationToken cancellationToken)
{
    var command = new ScanStudentQrCommand(
        request.TenantId,
        attendanceRecordId,
        request.QrToken,
        request.MarkedByUserId);

    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure || result.Value is null)
    {
        return BadRequest(new
        {
            Code = result.Error.Code,
            Message = result.Error.Message
        });
    }

    return Ok(result.Value);
}
}