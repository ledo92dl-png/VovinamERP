# Build 017 - Enrollment Engine

## Added

- Expanded `StudentClassEnrollment` into an enrollment lifecycle aggregate.
- Added enrollment lifecycle states: Trial, Active, Paused, Reserved, Transferred, Completed, Left, Archived.
- Added domain events for enrollment status changes and enrollment end.
- Added application draft service for preparing student-class enrollment.

## Business Rules Covered

- BR-STU-009: A student can enroll in many classes.
- BR-CLS-004: A student can study in many classes.
- BR-CLS-005: Student-class relationship is stored through enrollments.
- BR-CLS-006: Ending enrollment does not delete history.
