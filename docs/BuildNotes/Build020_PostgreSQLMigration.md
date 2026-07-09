# Build 020 - PostgreSQL Migration

## Added

- Docker Compose PostgreSQL service.
- EF Core design-time DbContext factory.

## Goal

Create the first PostgreSQL migration for VovinamERP.

## Commands

```powershell
cd C:\VovinamERP
docker compose up -d
cd C:\VovinamERP\backend
dotnet ef migrations add InitialCreate --project src\VovinamERP.Infrastructure --startup-project src\VovinamERP.Api --context VovinamDbContext --output-dir Persistence\Migrations
dotnet ef database update --project src\VovinamERP.Infrastructure --startup-project src\VovinamERP.Api --context VovinamDbContext
```
