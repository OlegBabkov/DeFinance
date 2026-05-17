---
name: add-feature
description: Add a new feature to DeFinance following clean architecture layer order (Domain → Application → Infrastructure → Api → Tests).
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
---

# Add Feature

Add a new feature to the DeFinance project following the clean architecture layer structure.

## Project Structure

- `src/DeFinance.Domain` — entities, value objects, domain events, interfaces
- `src/DeFinance.Application` — use cases, commands/queries (CQRS), DTOs, validators
- `src/DeFinance.Infrastructure` — EF Core repositories, external service adapters
- `src/DeFinance.Api` — ASP.NET Core 9 controllers/minimal API endpoints, DI wiring

## Steps

When asked to add a feature, follow this order:

1. **Domain** — add entity or value object if needed
2. **Application** — add command/query + handler + DTO + validator
3. **Infrastructure** — add or update repository implementation if persistence is involved
4. **Api** — add endpoint and register any new services in DI
5. **Tests** — add unit tests in the matching `tests/DeFinance.<Layer>.Tests` project

Keep each layer ignorant of layers above it. Domain has no references to Application or Infrastructure.
