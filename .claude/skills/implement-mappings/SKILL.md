---
name: implement-mappings
description: Detect new or modified Domain entities, create matching DTOs and mappings in the Application layer, then verify they are wired up correctly in the API and Application logic.
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
---

# Implement Mappings

Inspect recent changes to Domain entities, create the appropriate DTOs and mapping logic, and verify consistent usage across the Application and Api layers.

## Steps

### 1. Detect changed entities

Scan for new or modified files in:
- `src/DeFinance.Domain/Entities/` — identify which properties exist and their types

### 2. Create DTOs in the Application layer

Place DTOs under `src/DeFinance.Application/DTOs/<EntityName>/`:

- **`<EntityName>Response.cs`** — what the API returns; include all safe-to-expose properties
- **`Create<EntityName>Request.cs`** — payload for creation commands (omit `Id`, `IsActive`, computed fields)
- **`Update<EntityName>Request.cs`** — payload for update commands (only mutable fields)

DTOs should be plain records or classes with no Domain references. Keep nullability accurate.

### 3. Create mapping extensions

Check `src/DeFinance.Infrastructure/` and `src/DeFinance.Application/` for an existing mapping library:
- **Mapster** (`Mapster` / `MapsterMapper` NuGet) — add a `<EntityName>Mappings` register class
- **AutoMapper** (`AutoMapper` NuGet) — add a `<EntityName>Profile : Profile` class
- **None found** — create a static extension class `src/DeFinance.Application/DTOs/<EntityName>/<EntityName>MappingExtensions.cs` with:
  ```csharp
  public static <EntityName>Response ToResponse(this <EntityName> entity) => ...
  public static <EntityName> ToDomain(this Create<EntityName>Request request) => ...
  ```

If adding a mapping library for the first time, add the NuGet package to `src/DeFinance.Application/DeFinance.Application.csproj` and register it in `src/DeFinance.Infrastructure/InfrastructureServiceExtensions.cs`.

### 4. Verify Application layer usage

Check every command/query handler under `src/DeFinance.Application/` that touches the changed entity:
- Handlers should accept request DTOs (not raw Domain types) as input
- Handlers should return response DTOs (not raw Domain types) as output
- Mapping calls must use the mapping extensions or mapper service, not manual inline mapping

If a handler is missing, note it but do not create it — that is the job of the `add-feature` skill.

### 5. Verify Api layer usage

Check controllers or minimal-API endpoints under `src/DeFinance.Api/` for the changed entity:
- Action parameters should use request DTOs (not Domain entities)
- Action return types should use response DTOs
- No Domain types should leak into controller signatures

### 6. Report

Summarise:
- DTOs created and their file paths
- Mapping approach used (library or extension methods)
- Any handler or endpoint that was already correct vs. any that needs attention
- Any missing handler/endpoint that the user should create next

## Notes

- Never modify Domain entities from this skill — that belongs in the Domain layer.
- Do not create commands or query handlers — use the `add-feature` skill for that.
- If both request and response are identical (rare), still create separate types to allow them to diverge independently.
- Keep DTOs in the Application layer, never in Infrastructure or Domain.
