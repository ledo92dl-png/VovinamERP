# Build 025: Student REST API Foundation

## Added

- `StudentsController`
- `CreateStudentRequest`
- `StudentResponse`
- Basic REST endpoints:
  - `POST /api/students`
  - `GET /api/students`
  - `GET /api/students/{id}`
  - `DELETE /api/students/{id}`

## Notes

This build intentionally uses `VovinamDbContext` directly in the API controller as a first vertical slice to verify end-to-end persistence. Later builds will refactor this into CQRS handlers and repository-based application services.
