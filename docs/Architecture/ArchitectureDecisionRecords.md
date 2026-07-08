# Architecture Decision Records

## ADR-001: Use Multi-Tenant Architecture

VovinamERP is designed as a multi-tenant system.

Reason:
- One system can serve many clubs.
- Each tenant only sees its own data.
- The system can grow from one club to many clubs without redesign.

Decision:
- All business tables must include tenant_id.
- API queries must filter by current tenant.
- SuperAdmin may access multiple tenants when permitted.

---

## ADR-002: Use Person as the Human Data Root

Person is the root entity for human information.

Reason:
- A person may be a student today and an instructor later.
- Avoid duplicated personal information.
- Preserve lifelong martial history.

Decision:
- Student, Instructor, Guardian and UserAccount reference Person.

---

## ADR-003: Use Organization Tree

Organizations are stored as a hierarchy.

Examples:
- Federation
- Province
- Club
- Branch

Decision:
- Use one organizations table with parent_id.
- Use organization_type to distinguish levels.

---

## ADR-004: No Tournament Module in VovinamERP

VovinamERP does not organize tournaments.

Decision:
- No draw system.
- No bracket system.
- No match scoring.
- Only store achievements in student and instructor profiles.

---

## ADR-005: Instructor Assignment Is Session-Based

Instructors are not fixed permanently to a class.

Reason:
- In real club operation, instructors may change by session.
- A class may have many instructors or assistants.

Decision:
- TrainingSession has many SessionInstructors.
- TrainingClass may exist without fixed instructors.

---

## ADR-006: Monthly Tuition

Tuition is calculated by month.

Decision:
- TuitionInvoice is generated per student per month.
- Mid-month cases are handled by club decision and invoice notes.