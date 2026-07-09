# Build 012 - Person Lifecycle Foundation

## Summary

This build integrates the Person-centered lifecycle model into the actual source tree.

## Key decisions

- Person is the root human profile.
- Student, Instructor and Guardian reference Person.
- One person can become a student, instructor, guardian, staff or user over time.
- Student stores martial profile data, not duplicated personal data.
- Instructor stores professional teaching profile, not duplicated personal data.

## Out of scope

- EF Core mapping.
- Database migration.
- API endpoints.
- UI screens.
