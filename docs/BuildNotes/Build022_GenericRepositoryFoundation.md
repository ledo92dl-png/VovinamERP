# Build 022 - Generic Repository Foundation

## Added

- `IRepository<TEntity>` generic repository contract in Application layer.
- `EfRepository<TEntity>` Entity Framework Core implementation in Infrastructure layer.
- `AddRepositories()` extension for DI registration.

## Purpose

This build reduces duplicated repository code across modules such as Student, Instructor, Training, Attendance, Finance, Inventory and Salary.

## Rule

Specific repositories may still be added later when a module needs special queries, but standard CRUD-like access should go through the generic repository foundation.
