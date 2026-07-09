# Students API

## Create Student

`POST /api/students`

Creates a Person and Student record.

## List Students

`GET /api/students`

Optional query parameters:

- `tenantId`
- `organizationId`
- `status`

## Get Student by Id

`GET /api/students/{id}`

Returns one student profile by StudentId.

## Get Student by Member Number

`GET /api/students/by-member-number/{memberNumber}`

Returns one student profile by MemberNumber.

## Archive Student

`DELETE /api/students/{id}`

Archives the student instead of hard deleting.
