using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.Guardians;
using VovinamERP.Domain.Guardians;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/students/{studentId:guid}/guardians")]
public sealed class StudentGuardiansController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;

    public StudentGuardiansController(VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<StudentGuardianResponse>> LinkGuardian(
        Guid studentId,
        [FromBody] LinkStudentGuardianRequest request,
        CancellationToken cancellationToken)
    {
        var student = await _dbContext.Set<Student>()
            .FirstOrDefaultAsync(x => x.Id == studentId && !x.IsArchived, cancellationToken);

        if (student is null)
            return NotFound("Student was not found.");

        if (student.TenantId != request.TenantId)
            return BadRequest("Student does not belong to the specified tenant.");

        var guardian = await _dbContext.Set<Guardian>()
            .FirstOrDefaultAsync(x => x.Id == request.GuardianId && !x.IsArchived, cancellationToken);

        if (guardian is null)
            return NotFound("Guardian was not found.");

        if (guardian.TenantId != request.TenantId)
            return BadRequest("Guardian does not belong to the specified tenant.");

        var duplicated = await _dbContext.Set<StudentGuardian>()
            .AnyAsync(x =>
                x.StudentId == studentId &&
                x.GuardianId == request.GuardianId &&
                !x.IsArchived,
                cancellationToken);

        if (duplicated)
            return Conflict("This guardian is already linked to the student.");

        if (request.IsPrimary)
            await ClearPrimaryGuardianAsync(studentId, cancellationToken);

        var linkResult = StudentGuardian.Create(
            request.TenantId,
            studentId,
            request.GuardianId,
            request.Relationship,
            request.IsPrimary,
            request.Note);

        if (linkResult.IsFailure || linkResult.Value is null)
            return BadRequest(linkResult.Error);

        _dbContext.Set<StudentGuardian>().Add(linkResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var person = await _dbContext.Set<Person>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == guardian.PersonId, cancellationToken);

        if (person is null)
            return NotFound("Guardian person record was not found.");

        var response = StudentGuardianResponse.FromDomain(linkResult.Value, guardian, person);

        return CreatedAtAction(nameof(GetGuardians), new { studentId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentGuardianResponse>>> GetGuardians(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var studentExists = await _dbContext.Set<Student>()
            .AsNoTracking()
            .AnyAsync(x => x.Id == studentId && !x.IsArchived, cancellationToken);

        if (!studentExists)
            return NotFound("Student was not found.");

        var links = await (
            from link in _dbContext.Set<StudentGuardian>().AsNoTracking()
            join guardian in _dbContext.Set<Guardian>().AsNoTracking()
                on link.GuardianId equals guardian.Id
            join person in _dbContext.Set<Person>().AsNoTracking()
                on guardian.PersonId equals person.Id
            where link.StudentId == studentId && !link.IsArchived && !guardian.IsArchived
            orderby link.IsPrimary descending, person.FullName
            select StudentGuardianResponse.FromDomain(link, guardian, person))
            .ToListAsync(cancellationToken);

        return Ok(links);
    }

    [HttpPut("{guardianId:guid}")]
    public async Task<ActionResult<StudentGuardianResponse>> UpdateRelationship(
        Guid studentId,
        Guid guardianId,
        [FromBody] UpdateStudentGuardianRequest request,
        CancellationToken cancellationToken)
    {
        var link = await _dbContext.Set<StudentGuardian>()
            .FirstOrDefaultAsync(x =>
                x.StudentId == studentId &&
                x.GuardianId == guardianId &&
                !x.IsArchived,
                cancellationToken);

        if (link is null)
            return NotFound();

        if (request.IsPrimary)
            await ClearPrimaryGuardianAsync(studentId, cancellationToken);

        var updateResult = link.Update(
            request.Relationship,
            request.IsPrimary,
            request.Note,
            null);

        if (updateResult.IsFailure)
            return BadRequest(updateResult.Error);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var guardian = await _dbContext.Set<Guardian>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == guardianId, cancellationToken);

        if (guardian is null)
            return NotFound();

        var person = await _dbContext.Set<Person>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == guardian.PersonId, cancellationToken);

        if (person is null)
            return NotFound();

        return Ok(StudentGuardianResponse.FromDomain(link, guardian, person));
    }

    [HttpPatch("{guardianId:guid}/primary")]
    public async Task<IActionResult> SetPrimary(
        Guid studentId,
        Guid guardianId,
        CancellationToken cancellationToken)
    {
        var link = await _dbContext.Set<StudentGuardian>()
            .FirstOrDefaultAsync(x =>
                x.StudentId == studentId &&
                x.GuardianId == guardianId &&
                !x.IsArchived,
                cancellationToken);

        if (link is null)
            return NotFound();

        await ClearPrimaryGuardianAsync(studentId, cancellationToken);

        var result = link.SetPrimary(true, null);

        if (result.IsFailure)
            return BadRequest(result.Error);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{guardianId:guid}")]
    public async Task<IActionResult> UnlinkGuardian(
        Guid studentId,
        Guid guardianId,
        CancellationToken cancellationToken)
    {
        var link = await _dbContext.Set<StudentGuardian>()
            .FirstOrDefaultAsync(x =>
                x.StudentId == studentId &&
                x.GuardianId == guardianId &&
                !x.IsArchived,
                cancellationToken);

        if (link is null)
            return NotFound();

        link.Archive(null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task ClearPrimaryGuardianAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var primaryLinks = await _dbContext.Set<StudentGuardian>()
            .Where(x => x.StudentId == studentId && x.IsPrimary && !x.IsArchived)
            .ToListAsync(cancellationToken);

        foreach (var primaryLink in primaryLinks)
            primaryLink.SetPrimary(false, null);
    }
}
