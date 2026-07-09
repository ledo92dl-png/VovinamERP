# Build 034: Training Class API Module

## Added

- Training class request/response contracts.
- Training class list query with pagination and filtering.
- TrainingClassesController.

## API Endpoints

- `POST /api/training-classes`
- `GET /api/training-classes`
- `GET /api/training-classes/{id}`
- `PUT /api/training-classes/{id}`
- `DELETE /api/training-classes/{id}`

## Business Notes

- Training class belongs to one tenant.
- Training class belongs to one organization/branch.
- Training class code is unique within a tenant at API validation level.
- Archive uses soft delete; data history is preserved.

## Next Build

Build 035 should add Training Session API foundation.
