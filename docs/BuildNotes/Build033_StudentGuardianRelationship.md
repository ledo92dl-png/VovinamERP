# Build 033: Student Guardian Relationship

## Added
- StudentGuardian domain relationship entity.
- StudentGuardian relationship types.
- EF Core mapping for student_guardians.
- Student-to-guardian linking API.
- List guardians of a student.
- Update relationship metadata.
- Set primary guardian.
- Unlink guardian using soft archive.

## Endpoints
- POST /api/students/{studentId}/guardians
- GET /api/students/{studentId}/guardians
- PUT /api/students/{studentId}/guardians/{guardianId}
- PATCH /api/students/{studentId}/guardians/{guardianId}/primary
- DELETE /api/students/{studentId}/guardians/{guardianId}

## Notes
- One student can have many guardians.
- One guardian can be linked to many students.
- Only one primary guardian should be active for one student.
- This build adds model changes. A follow-up migration build should update PostgreSQL schema.
