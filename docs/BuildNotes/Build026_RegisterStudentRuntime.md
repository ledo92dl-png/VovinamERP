# Build 026: Register Student Runtime Wiring

## Added

- API runtime wiring in `Program.cs`.
- Application service registration.
- Infrastructure service registration.
- Repository service registration.
- Local CORS policy for future React/Flutter clients.
- Development connection string.
- Student API usage document.

## Result

The API is now wired to controllers, Application, Infrastructure, EF Core, and PostgreSQL.

## Check

```powershell
cd C:\VovinamERP\backend
dotnet build
```

Expected:

```text
Build succeeded
```
