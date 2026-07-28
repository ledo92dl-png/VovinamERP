using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Dashboard.GetDashboardSummary;

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
}