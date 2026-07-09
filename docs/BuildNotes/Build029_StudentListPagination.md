# Build 029: Student List API + Pagination

## Added

- `PagedResult<T>` contract.
- `StudentListQuery` contract.
- Paged `GET /api/students` endpoint.

## Updated

- `StudentsController`
- `StudentsApi.md`

## Features

- Pagination with `page` and `pageSize`.
- Keyword search by full name, member number and martial name.
- Filters by tenant, organization, current belt rank and student status.
- Sorting by full name, member number, enrollment date or status.

## Notes

This build keeps the current direct `VovinamDbContext` API slice. Later builds can move list/query logic into CQRS handlers without changing the public API contract.
