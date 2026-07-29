using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Dashboard.GetDashboardSummary;
using VovinamERP.Application.Dashboard.GetStudentsByBelt;
using VovinamERP.Application.Dashboard.GetAttendanceTrend;
using VovinamERP.Application.Dashboard.GetRevenueSummary;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<GetDashboardSummaryResult>>
        GetSummary(
            [FromQuery] Guid tenantId,
            [FromQuery] DateOnly? reportDate,
            CancellationToken cancellationToken)
    {
        var resolvedReportDate =
            reportDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = new GetDashboardSummaryQuery(
            tenantId,
            resolvedReportDate);

        var result = await _sender.Send(
            query,
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("students-by-belt")]
public async Task<ActionResult<GetStudentsByBeltResult>>
    GetStudentsByBelt(
        [FromQuery] Guid tenantId,
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
{
    var query = new GetStudentsByBeltQuery(
        tenantId,
        organizationId);

    var result = await _sender.Send(
        query,
        cancellationToken);

    return Ok(result);
}
    [HttpGet("attendance-trend")]
public async Task<ActionResult<GetAttendanceTrendResult>>
    GetAttendanceTrend(
        [FromQuery] Guid tenantId,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
{
    var query = new GetAttendanceTrendQuery(
        tenantId,
        fromDate,
        toDate,
        organizationId);

    var result = await _sender.Send(
        query,
        cancellationToken);

    return Ok(result);
}
    [HttpGet("revenue-summary")]
public async Task<ActionResult<GetRevenueSummaryResult>>
    GetRevenueSummary(
        [FromQuery] Guid tenantId,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
{
    var query = new GetRevenueSummaryQuery(
        tenantId,
        fromDate,
        toDate,
        organizationId);

    var result = await _sender.Send(
        query,
        cancellationToken);

    return Ok(result);
}
}