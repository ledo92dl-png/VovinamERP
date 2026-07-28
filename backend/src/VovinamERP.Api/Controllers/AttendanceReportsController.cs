using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Attendance.GetCrossLocationAttendanceReport;
using VovinamERP.Application.Attendance.GetCrossLocationByOrganizationReport;
using VovinamERP.Application.Attendance.GetCrossLocationByStudentReport;
using VovinamERP.Application.Attendance.GetCrossLocationAttendanceDetails;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/attendance-reports")]
public sealed class AttendanceReportsController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("cross-location")]
    public async Task<ActionResult<GetCrossLocationAttendanceReportResult>>
        GetCrossLocationReport(
            [FromQuery] Guid tenantId,
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            CancellationToken cancellationToken)
    {
        var query =
            new GetCrossLocationAttendanceReportQuery(
                tenantId,
                fromDate,
                toDate);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("cross-location/by-organization")]
public async Task<ActionResult<GetCrossLocationByOrganizationReportResult>>
    GetCrossLocationByOrganizationReport(
        [FromQuery] Guid tenantId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
{
    var query =
        new GetCrossLocationByOrganizationReportQuery(
            tenantId,
            fromDate,
            toDate);

    var result = await _sender.Send(
        query,
        cancellationToken);

    return Ok(result);
}
[HttpGet("cross-location/by-student")]
public async Task<ActionResult<GetCrossLocationByStudentReportResult>>
    GetCrossLocationByStudentReport(
        [FromQuery] Guid tenantId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
{
    var query =
        new GetCrossLocationByStudentReportQuery(
            tenantId,
            fromDate,
            toDate);

    var result = await _sender.Send(
        query,
        cancellationToken);

    return Ok(result);
}
    [HttpGet("cross-location/details")]
public async Task<ActionResult<GetCrossLocationAttendanceDetailsResult>>
    GetCrossLocationAttendanceDetails(
        [FromQuery] Guid tenantId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] Guid? studentId,
        [FromQuery] Guid? trainingOrganizationId,
        CancellationToken cancellationToken)
{
    var query =
        new GetCrossLocationAttendanceDetailsQuery(
            tenantId,
            fromDate,
            toDate,
            studentId,
            trainingOrganizationId);

    var result = await _sender.Send(
        query,
        cancellationToken);

    return Ok(result);
}
}