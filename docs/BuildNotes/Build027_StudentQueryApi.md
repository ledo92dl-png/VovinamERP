# Build 027 - Student Query API

## Added

- Enhanced `GET /api/students` with optional filters:
  - `tenantId`
  - `organizationId`
  - `status`
- Added `GET /api/students/by-member-number/{memberNumber}`.
- Kept existing `GET /api/students/{id}` endpoint.

## Purpose

This build strengthens the Student REST API so the system can retrieve student records after registration.

## Notes

This is still a foundation API. Later builds will move query handling into CQRS handlers instead of using `DbContext` directly inside the controller.
