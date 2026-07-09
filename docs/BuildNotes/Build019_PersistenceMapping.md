# Build 019 - EF Core Mapping Foundation

## Added

- `VovinamDbContext`
- EF Core configurations for:
  - Tenant
  - Organization
  - Person
  - Student
  - BeltRank
- Infrastructure dependency injection entry point

## Notes

This build starts the EF Core persistence mapping layer. Full mapping for Training, Finance, Promotion and Achievement will be expanded in subsequent builds before the first migration.
