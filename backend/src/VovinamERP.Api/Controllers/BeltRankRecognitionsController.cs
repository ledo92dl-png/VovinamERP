using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Contracts.BeltRecognitions;
using VovinamERP.Application.BeltRecognitions.AddBeltRankDocument;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/belt-rank-recognitions")]
public sealed class BeltRankRecognitionsController : ControllerBase
{
    private readonly ISender _sender;

    public BeltRankRecognitionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{recognitionId:guid}/document")]
    public async Task<IActionResult> AddDocument(
        Guid recognitionId,
        [FromBody] AddBeltRankDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddBeltRankDocumentCommand(
            request.TenantId,
            recognitionId,
            request.DocumentType,
            request.DocumentNumber,
            request.SignedDate,
            request.ScanUrl,
            request.Note,
            null);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                Code = result.Error.Code,
                Message = result.Error.Message
            });
        }

        return Created(
            $"/api/belt-rank-recognitions/{recognitionId}/document?tenantId={request.TenantId}",
            new
            {
                Id = result.Value
            });
    }
}