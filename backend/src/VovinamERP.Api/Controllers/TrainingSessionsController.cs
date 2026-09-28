using MediatR;
using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Contracts.TrainingSessions;
using VovinamERP.Application.Training.CreateTrainingSession;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/training-sessions")]
public sealed class TrainingSessionsController : ControllerBase
{
    private readonly ISender _sender;

    public TrainingSessionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTrainingSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTrainingSessionCommand(
            request.TenantId,
            request.TrainingClassId,
            request.SessionDate,
            request.StartTime,
            request.EndTime,
            request.LessonPlan,
            request.CoachNote);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Code == "TRAINING_031")
            {
                return Conflict(new
                {
                    Code = result.Error.Code,
                    Message = result.Error.Message
                });
            }

            return BadRequest(new
            {
                Code = result.Error.Code,
                Message = result.Error.Message
            });
        }

        return Created(
            $"/api/training-sessions/{result.Value}",
            new
            {
                Id = result.Value
            });
    }
}