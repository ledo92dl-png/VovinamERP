# Build 015 - Finance / Tuition Foundation

## Added

- TuitionInvoice aggregate
- TuitionPayment entity
- Receipt aggregate
- TuitionInvoiceStatus
- TuitionPaymentMethod
- ReceiptStatus
- FinanceErrors
- FinanceEvents

## Business rules implemented

- BR-FIN-001: Tuition is monthly.
- BR-FIN-002: A student can have many monthly tuition invoices.
- BR-FIN-003: Confirmed receipt cannot be edited.
- BR-FIN-005: Payment history is not deleted.
- BR-FIN-006: Tuition invoice can be paid once or multiple times.
- BR-FIN-008: Mid-month cases are handled by note.
