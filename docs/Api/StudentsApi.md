# Students API

## Register student

Endpoint:

```http
POST /api/students
```

Purpose:

- Create `Person`.
- Create `Student` linked to that `Person`.
- Persist both records to PostgreSQL.

Sample body:

```json
{
  "tenantId": "00000000-0000-0000-0000-000000000000",
  "organizationId": "00000000-0000-0000-0000-000000000000",
  "fullName": "Nguyen Van A",
  "gender": 1,
  "dateOfBirth": "2012-05-10",
  "phoneNumber": "0909000000",
  "email": null,
  "address": "Dong Nai",
  "avatarUrl": null,
  "currentBeltRankId": null,
  "enrollmentDate": "2026-07-09",
  "martialName": null,
  "introducedBy": null,
  "martialProfileNote": "Nhap mon dot dau tien"
}
```

> Note: Replace `tenantId` and `organizationId` with real IDs seeded in the database.
