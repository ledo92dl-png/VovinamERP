using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Finance.GetStudentReceiptHistory;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/students/{studentId:guid}/receipts")]
public sealed class StudentReceiptsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentReceiptsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(GetStudentReceiptHistoryResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistory(
        Guid studentId,
        [FromQuery] Guid tenantId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? itemType,
        [FromQuery] string? receiptStatus,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query =
            new GetStudentReceiptHistoryQuery(
                tenantId,
                studentId,
                fromDate,
                toDate,
                itemType,
                receiptStatus,
                pageNumber,
                pageSize);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure ||
            result.Value is null)
        {
            return BadRequest(new
            {
                error = result.Error.Message
            });
        }

        return Ok(result.Value);
    }
}