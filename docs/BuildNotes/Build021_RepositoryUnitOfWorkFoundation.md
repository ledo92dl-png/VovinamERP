# Build 021 - Repository + Unit of Work Foundation

## Added

- Generic repository interface for aggregate roots.
- Person repository interface and implementation.
- Student repository interface and implementation.
- Repository dependency injection extension.

## Purpose

This build prepares the Application layer to work through repository abstractions instead of depending directly on EF Core.

## Rules

- Application layer depends on repository interfaces.
- Infrastructure layer implements repository interfaces.
- Unit of Work remains handled by VovinamDbContext.
