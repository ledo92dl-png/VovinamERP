# Build 032: Guardian API Module

## Added
- Guardian REST API foundation.
- Create guardian with shared Person record.
- Get guardian by id.
- List guardians with pagination and keyword search.
- Update guardian personal information and relationship note.
- Archive guardian using soft delete.

## Endpoints
- POST /api/guardians
- GET /api/guardians
- GET /api/guardians/{id}
- PUT /api/guardians/{id}
- DELETE /api/guardians/{id}

## Notes
- Guardian uses Person as the human data root.
- Student-to-guardian linking will be handled in the next build.
