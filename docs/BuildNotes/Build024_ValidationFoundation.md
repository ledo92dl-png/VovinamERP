# Build 024 - Validation Foundation

## Added

- FluentValidation pipeline behavior foundation.
- Application-level validation exception.
- Shared validation structure for future CQRS commands and queries.

## Purpose

All future command/query validation should be handled before reaching business handlers.

## Notes

This build prepares the application layer for validators such as:

- RegisterStudentCommandValidator
- EnrollStudentInClassCommandValidator
- CreateTuitionInvoiceCommandValidator
