# Students API

## Create Student

`POST /api/students`

Creates a Person and Student record.

## List Students

`GET /api/students`

Returns a paged student list.

Query parameters:

- `tenantId`
- `organizationId`
- `currentBeltRankId`
- `status`
- `keyword`
- `sortBy`
- `descending`
- `page`
- `pageSize`

Examples:

`GET /api/students?page=1&pageSize=20`

`GET /api/students?keyword=Nguyen`

`GET /api/students?status=Active`

`GET /api/students?sortBy=enrollmentDate&descending=true`

## Get Student by Id

`GET /api/students/{id}`

Returns one student profile by StudentId.

## Get Student by Member Number

`GET /api/students/by-member-number/{memberNumber}`

Returns one student profile by MemberNumber.

## Update Student

`PUT /api/students/{id}`

Updates personal information and martial profile fields.

## Archive Student

`DELETE /api/students/{id}`

Archives the student instead of hard deleting.
