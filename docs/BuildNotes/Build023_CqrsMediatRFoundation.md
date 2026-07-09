# Build 023 - CQRS + MediatR Foundation

## Added

- MediatR package for the Application layer.
- CQRS messaging abstractions:
  - `ICommand`
  - `ICommand<TResponse>`
  - `ICommandHandler<TCommand>`
  - `ICommandHandler<TCommand, TResponse>`
  - `IQuery<TResponse>`
  - `IQueryHandler<TQuery, TResponse>`
- Application dependency injection extension.
- Basic MediatR pipeline behavior placeholder.

## Purpose

This build prepares VovinamERP for feature-based application flows.
Future features will follow this flow:

```text
API Controller
    -> Command / Query
    -> MediatR
    -> Handler
    -> Repository
    -> UnitOfWork
    -> Database
```

## Notes

Validation will be added in the next build using FluentValidation.
