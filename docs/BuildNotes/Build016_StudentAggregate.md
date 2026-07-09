# Build 016 - Student Aggregate Root

## Added

- Strengthened Student aggregate root.
- Added registration factory method.
- Added martial profile update behavior.
- Added status change behavior.
- Added current belt change behavior.
- Added domain events for student lifecycle.

## Business rules covered

- BR-STU-002: Student belongs to one tenant.
- BR-STU-003: Member number is generated/managed by system.
- BR-STU-006: Enrollment date is required.
- BR-STU-008: Student is archived, not deleted.
- BR-STU-010: Student has one current belt rank.
- BR-STU-011: Belt change raises a lifecycle event.
