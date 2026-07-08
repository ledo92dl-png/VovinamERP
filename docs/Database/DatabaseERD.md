# VovinamERP Database ERD

## 1. Core Structure

```text
tenants
   |
   +-- organizations
   |
   +-- persons
   |       |
   |       +-- students
   |       +-- instructors
   |       +-- guardians
   |       +-- users
   |
   +-- belt_ranks
   |
   +-- training_classes
   |
   +-- training_sessions
```

## 2. Organization Relationship

```text
tenants
   |
   +-- organizations
           |
           +-- organizations.parent_id
```

Rules:
- organizations.parent_id is nullable.
- organization_type defines Federation, Province, Club, Branch.
- Branch belongs under Club.
- Club can belong under Province.
- Province can belong under Federation.

## 3. Person Relationship

```text
persons
   |
   +-- students
   |
   +-- instructors
   |
   +-- guardians
   |
   +-- users
```

Rules:
- One Person may become Student, Instructor, Guardian or User.
- Personal information is stored once in persons.
- Role-specific information is stored in related tables.

## 4. Student and Class Relationship

```text
students
   |
   +-- student_class_enrollments
                |
                +-- training_classes
```

Rules:
- A student can enroll in many classes.
- A class can have many students.
- Enrollment has start_date, end_date and status.
- Ending enrollment does not delete history.

## 5. Session and Instructor Relationship

```text
training_classes
   |
   +-- training_sessions
             |
             +-- session_instructors
                        |
                        +-- instructors
```

Rules:
- A training session can have many instructors.
- Instructor assignment is recorded per session.
- Class instructors are not fixed.

## 6. Attendance Relationship

```text
training_sessions
   |
   +-- attendance_records
             |
             +-- attendance_details
                        |
                        +-- students
```

Rules:
- One session has one attendance record.
- Each attendance detail belongs to one student.
- A student cannot be duplicated in one attendance record.

## 7. Belt Relationship

```text
belt_ranks
   |
   +-- student_belt_histories
              |
              +-- students
```

Rules:
- Student has current_belt_rank_id.
- Belt changes must create student_belt_history.
- Belt history is never deleted.

## 8. Promotion Relationship

```text
promotion_exams
   |
   +-- promotion_candidates
              |
              +-- promotion_results
              |
              +-- certificates
```

Rules:
- Failed exams are stored.
- Passed exams update current belt and belt history.
- Results are never deleted.

## 9. Finance Relationship

```text
students
   |
   +-- tuition_invoices
              |
              +-- tuition_payments
              |
              +-- receipts
```

Rules:
- Tuition is monthly.
- Invoice may be paid once or many times.
- Confirmed receipts are not edited.

## 10. Achievement Relationship

```text
achievements
   |
   +-- owner_type
   +-- owner_id
```

Rules:
- Achievement may belong to Student or Instructor.
- VovinamERP only stores achievements.
- Tournament organization is out of scope.

## 11. System Relationship

```text
timeline_events
audit_logs
attachments
notifications
```

Rules:
- Timeline stores business events.
- Audit logs store data changes.
- Attachments can link to many entity types.