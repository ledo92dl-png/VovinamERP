using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Controllers.Receipts;
using VovinamERP.Application.Finance.CreateReceipt;
using VovinamERP.Application.Finance.AddReceiptItem;
using VovinamERP.Application.Finance.ConfirmReceipt;
using VovinamERP.Application.Finance.GetReceiptById;
using VovinamERP.Application.Finance.GetReceipts;
using VovinamERP.Application.Finance.AssignStudentToReceiptItem;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/receipts")]
public sealed class ReceiptsController : ControllerBase
{
    private readonly ISender _sender;

    public ReceiptsController(ISender sender)
    {
        _sender = sender;
    }

[HttpPut("{receiptId:guid}/items/{receiptItemId:guid}/student")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> AssignStudent(
    Guid receiptId,
    Guid receiptItemId,
    [FromBody] AssignStudentToReceiptItemRequest request,
    CancellationToken cancellationToken)
{
    var command = new AssignStudentToReceiptItemCommand(
        request.TenantId,
        receiptId,
        receiptItemId,
        request.StudentId);

    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure)
    {
        if (result.Error.Code == "FIN_030" ||
            result.Error.Code == "FIN_021")
        {
            return NotFound(new
            {
                error = result.Error.Message
            });
        }

        return BadRequest(new
        {
            error = result.Error.Message
        });
    }

    return Ok(new
    {
        receiptId,
        receiptItemId,
        studentId = request.StudentId
    });
}

  [HttpGet]
[ProducesResponseType(
    typeof(GetReceiptsResult),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    StatusCodes.Status400BadRequest)]
public async Task<IActionResult> GetReceipts(
    [FromQuery] Guid tenantId,
    [FromQuery] DateOnly? fromDate,
    [FromQuery] DateOnly? toDate,
    [FromQuery] string? status,
    [FromQuery] string? paymentMethod,
    [FromQuery] Guid? collectedByUserId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
{
    var query = new GetReceiptsQuery(
        tenantId,
        fromDate,
        toDate,
        status,
        paymentMethod,
        collectedByUserId,
        pageNumber,
        pageSize);

    var result = await _sender.Send(
        query,
        cancellationToken);

    if (result.IsFailure || result.Value is null)
    {
        return BadRequest(new
        {
            error = result.Error.Message
        });
    }

    return Ok(result.Value);
}


[HttpGet("{receiptId:guid}")]
[ProducesResponseType(
    typeof(ReceiptDetailsDto),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetById(
    Guid receiptId,
    [FromQuery] Guid tenantId,
    CancellationToken cancellationToken)
{
    var query = new GetReceiptByIdQuery(
        tenantId,
        receiptId);

    var result = await _sender.Send(
        query,
        cancellationToken);

    if (result.IsFailure || result.Value is null)
    {
        if (result.Error.Code == "FIN_050")
        {
            return NotFound(new
            {
                error = result.Error.Message
            });
        }

        return BadRequest(new
        {
            error = result.Error.Message
        });
    }

    return Ok(result.Value);
}


[HttpPost("{receiptId:guid}/items")]
[ProducesResponseType(
    typeof(AddReceiptItemResult),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    StatusCodes.Status400BadRequest)]
[ProducesResponseType(
    StatusCodes.Status404NotFound)]
public async Task<IActionResult> AddItem(
    Guid receiptId,
    [FromBody] AddReceiptItemRequest request,
    CancellationToken cancellationToken)
{
    
    var command = new AddReceiptItemCommand(
        request.TenantId,
        receiptId,
        request.StudentId,
        request.ItemType,
        request.ReferenceId,
        request.Description,
        request.Quantity,
        request.UnitPrice,
        request.DiscountAmount,
        request.Note);

    
    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure || result.Value is null)
    {
        return BadRequest(new
        {
            error = result.Error.Message
        });
    }

    return Ok(result.Value);
}

    [HttpPost("{receiptId:guid}/confirm")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> Confirm(
    Guid receiptId,
    [FromQuery] Guid tenantId,
    CancellationToken cancellationToken)
{
    var command = new ConfirmReceiptCommand(
        tenantId,
        receiptId);

    var result = await _sender.Send(
        command,
        cancellationToken);

    if (result.IsFailure)
    {
        return BadRequest(new
        {
            error = result.Error.Message
        });
    }

    return Ok(new
    {
        receiptId,
        status = "Confirmed"
    });
}

    [HttpPost]
    [ProducesResponseType(
        typeof(CreateReceiptResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateReceiptResult>> Create(
        [FromBody] CreateReceiptRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReceiptCommand(
            request.TenantId,
            request.CollectedByUserId,
            request.ReceiptNumber,
            request.PaymentMethod,
            request.ReceiptDate,
            request.TransactionReference,
            request.EvidenceImageUrl,
            request.Note);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(Create),
            new
            {
                id = result.ReceiptId
            },
            result);
    }
}