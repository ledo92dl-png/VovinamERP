# Build 028: Update Student API

## Added

- `UpdateStudentRequest`
- `PUT /api/students/{id}` endpoint

## Updated

- `StudentsController`
- `StudentsApi.md`

## Business Rules

- Student update must stay within the same tenant.
- Personal information is updated through `Person.UpdateBasicInfo`.
- Martial profile is updated through `Student.UpdateMartialProfile`.
- Belt update uses `Student.ChangeCurrentBelt` so domain events are raised.
