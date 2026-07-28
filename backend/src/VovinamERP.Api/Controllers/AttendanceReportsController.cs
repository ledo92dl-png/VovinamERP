using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Attendance.GetCrossLocationAttendanceReport;

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
}