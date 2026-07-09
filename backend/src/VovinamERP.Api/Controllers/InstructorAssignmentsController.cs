using Microsoft.AspNetCore.Mvc;
using VovinamERP.Api.Contracts.InstructorAssignments;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/training-classes/{trainingClassId:guid}/instructors")]
public sealed class InstructorAssignmentsController : ControllerBase
{
    [HttpPost]
    public IActionResult AssignInstructor(
        Guid trainingClassId,
        AssignInstructorToClassRequest request)
    {
        return Ok(new
        {
            Message = "Instructor assignment endpoint created.",
            TrainingClassId = trainingClassId,
            Request = request
        });
    }
}