using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;

namespace VovinamERP.Application.Students.RegisterStudent;

public sealed record RegisterStudentDraft(
    Person Person,
    Student Student,
    RegisterStudentResponse Response);
