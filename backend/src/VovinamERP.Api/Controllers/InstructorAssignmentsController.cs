using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Contracts.InstructorAssignments;
using VovinamERP.Application.InstructorAssignments.AssignInstructorToClass;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/training-classes/{trainingClassId:guid}/instructors")]
public sealed class InstructorAssignmentsController : ControllerBase
{
    private readonly AssignInstructorToClassService _service;

    public InstructorAssignmentsController(AssignInstructorToClassService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> AssignInstructor(
        Guid trainingClassId,
        AssignInstructorToClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignInstructorToClassCommand(
            request.TenantId,
            trainingClassId,
            request.InstructorId,
            request.Role,
            request.StartDate,
            request.EndDate,
            request.Note);

        var result = await _service.HandleAsync(command, cancellationToken);

        return Ok(result);
    }
}