# Build 014 - Training Module Foundation

## Added

- TrainingClass aggregate
- StudentClassEnrollment aggregate
- TrainingSession aggregate
- SessionInstructor aggregate
- AttendanceRecord aggregate
- AttendanceDetail aggregate
- Training enums
- Training business errors
- Training domain events

## Business Rules Covered

- A student can enroll in multiple classes.
- A class can have many students.
- Instructors are assigned by training session, not fixed permanently to a class.
- A training session can have many instructors.
- Attendance is recorded per session and per student.
- Attendance statuses include Present, Late, Excused and Absent.
