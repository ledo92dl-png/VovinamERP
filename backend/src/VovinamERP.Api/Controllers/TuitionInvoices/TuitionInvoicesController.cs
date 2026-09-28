using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Application.Finance.ApplyStudentCreditToTuitionInvoice;
using VovinamERP.Application.Finance.GenerateMonthlyTuitionInvoice;
using VovinamERP.Application.Finance.ApplySpecialTuitionDiscount;

namespace VovinamERP.Api.Controllers.TuitionInvoices;

[ApiController]
[Route("api/tuition-invoices")]
public sealed class TuitionInvoicesController : ControllerBase
{
    private readonly ISender _sender;

    public TuitionInvoicesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(GenerateMonthlyTuitionInvoiceResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerateMonthlyTuitionInvoiceResult>>
        GenerateMonthly(
            [FromBody] GenerateMonthlyTuitionInvoiceRequest request,
            CancellationToken cancellationToken)
    {
        var command =
            new GenerateMonthlyTuitionInvoiceCommand(
                request.TenantId,
                request.StudentId,
                request.InvoiceNumber,
                request.Year,
                request.Month,
                request.StandardAmount,
                request.Note);

        var result = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GenerateMonthly),
            new
            {
                id = result.TuitionInvoiceId
            },
            result);
    }
	[HttpPost("{tuitionInvoiceId:guid}/apply-credit")]
[ProducesResponseType(
    typeof(ApplyStudentCreditToTuitionInvoiceResult),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ApplyStudentCreditToTuitionInvoiceResult>>
    ApplyCredit(
        Guid tuitionInvoiceId,
        [FromBody] ApplyStudentCreditRequest request,
        CancellationToken cancellationToken)
{
    var command =
        new ApplyStudentCreditToTuitionInvoiceCommand(
            request.TenantId,
            request.StudentId,
            tuitionInvoiceId,
            request.Amount,
            request.TransactionDate,
            request.Note);

    var result = await _sender.Send(
        command,
        cancellationToken);

    return Ok(result);
}

    [HttpPost("{tuitionInvoiceId:guid}/special-discount")]
[ProducesResponseType(
    typeof(ApplySpecialTuitionDiscountResult),
    StatusCodes.Status200OK)]
[ProducesResponseType(
    StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ApplySpecialTuitionDiscountResult>>
    ApplySpecialDiscount(
        Guid tuitionInvoiceId,
        [FromBody] ApplySpecialTuitionDiscountRequest request,
        CancellationToken cancellationToken)
{
    var command =
    new ApplySpecialTuitionDiscountCommand(
        request.TenantId,
        request.StudentId,
        tuitionInvoiceId,
        request.DiscountType,
        request.DiscountValue,
        request.Reason,
        request.ApprovedByUserId);

    var result = await _sender.Send(
        command,
        cancellationToken);

    return Ok(result);
}

}