# Build 030: Archive Student API

## Added

- `PATCH /api/students/{id}/archive`
- `DELETE /api/students/{id}` as a soft-delete alias
- `ArchiveStudentRequest`

## Business Rules

- Students are not physically deleted.
- Archive uses the Student aggregate `Archive()` method.
- Archived students remain in database for history and auditability.

## Notes

This build intentionally keeps the API implementation simple and uses `VovinamDbContext` directly. In later builds this endpoint can be refactored to a full CQRS command handler when the archive flow needs audit reasons, permission checks, and tenant enforcement.
