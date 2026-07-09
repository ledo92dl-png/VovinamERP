# Build 013 - Student Registration Foundation

## Added

- Student registration application service.
- Student registration command, response and draft objects.
- Student registration code generator interface.
- Person, Student, Instructor and Guardian domain module foundation files.

## Purpose

This build prepares the first real business workflow: registering a new Vovinam student.

The workflow follows the Person-as-root decision:

1. Create Person.
2. Create Student linked to Person.
3. Generate visible PersonCode and MemberNumber through an external generator.
4. Return a registration draft that can later be persisted by Infrastructure.

## Notes

This build does not write to database yet. Persistence will be added after EF Core and repository setup.
