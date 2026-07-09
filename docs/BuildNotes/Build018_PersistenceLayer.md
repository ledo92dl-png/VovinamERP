# Build 018 - Persistence Layer Foundation

## Added

- `VovinamDbContext` in Infrastructure.
- EF Core and PostgreSQL package references.
- Basic entity configurations for Tenant, Organization, Person, Student, and BeltRank.
- `IUnitOfWork` application interface.
- `ICurrentTenantProvider` application interface.
- Infrastructure dependency injection extension.

## Fixed

- Renamed `AttendanceRecord.CreatedBy` to `CreatedByUserId` to avoid hiding `AuditableEntity.CreatedBy`.

## Notes

This build starts the persistence layer but does not create database migrations yet.
The first migration will be created after the core mappings are stable.
