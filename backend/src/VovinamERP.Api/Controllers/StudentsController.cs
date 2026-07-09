using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VovinamERP.Api.Contracts.Students;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using VovinamERP.Infrastructure.Persistence;

namespace VovinamERP.Api.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly VovinamDbContext _dbContext;

    public StudentsController(VovinamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Create(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var personCode = $"P-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var memberNumber = $"MS-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var personResult = Person.Create(
            request.TenantId,
            personCode,
            request.FullName,
            request.Gender,
            request.DateOfBirth,
            request.PhoneNumber,
            request.Email,
            request.Address,
            request.AvatarUrl);

        if (personResult.IsFailure || personResult.Value is null)
            return BadRequest(personResult.Error);

        var studentResult = Student.Register(
            request.TenantId,
            personResult.Value.Id,
            request.OrganizationId,
            request.CurrentBeltRankId,
            memberNumber,
            request.EnrollmentDate,
            request.MartialName,
            request.IntroducedBy,
            request.MartialProfileNote);

        if (studentResult.IsFailure || studentResult.Value is null)
            return BadRequest(studentResult.Error);

        _dbContext.Set<Person>().Add(personResult.Value);
        _dbContext.Set<Student>().Add(studentResult.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = ToResponse(studentResult.Value, personResult.Value);

        return CreatedAtAction(nameof(GetById), new { id = response.StudentId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var query =
            from student in _dbContext.Set<Student>().AsNoTracking()
            join person in _dbContext.Set<Person>().AsNoTracking()
                on student.PersonId equals person.Id
            orderby person.FullName
            select ToResponse(student, person);

        var students = await query.ToListAsync(cancellationToken);

        return Ok(students);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Set<Student>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
            return NotFound();

        var person = await _dbContext.Set<Person>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == student.PersonId, cancellationToken);

        if (person is null)
            return NotFound();

        return Ok(ToResponse(student, person));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Set<Student>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (student is null)
            return NotFound();

        student.Archive(null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static StudentResponse ToResponse(Student student, Person person)
    {
        return new StudentResponse(
            student.Id,
            student.PersonId,
            student.TenantId,
            student.OrganizationId,
            student.MemberNumber,
            person.FullName,
            person.Gender,
            person.DateOfBirth,
            person.PhoneNumber,
            person.Email,
            person.Address,
            student.CurrentBeltRankId,
            student.EnrollmentDate,
            student.Status,
            student.MartialName,
            student.IntroducedBy,
            student.MartialProfileNote);
    }
}
