using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.Students;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentArchiveController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;

    public StudentArchiveController(VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> Archive(
        Guid id,
        [FromBody] ArchiveStudentRequest? request,
        CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
            return NotFound();

        student.Archive(request?.UserId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
            return NotFound();

        student.Archive(null);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
