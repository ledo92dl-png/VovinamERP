using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Contracts.Attendance;
using VovinamERP.Application.Attendance.MarkStudentAttendance;
using VovinamERP.Application.Attendance.UpdateStudentAttendance;
using VovinamERP.Application.Attendance.GetAttendanceRecord;
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
}