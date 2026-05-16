---
name: create-migration
description: Detect entity/configuration changes in the DeFinance Domain and Infrastructure projects and create a named EF Core migration.
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
---

# Create Migration

Inspect recent code changes in the Domain and Infrastructure layers, derive a meaningful migration name, then generate the EF Core migration.

## Steps

1. **Find what changed** — look at modified or new files in these locations:
   - `src/DeFinance.Domain/Entities/` — new or modified entities
   - `src/DeFinance.Infrastructure/Persistence/Configurations/` — new or modified `IEntityTypeConfiguration<T>` classes
   - `src/DeFinance.Infrastructure/Persistence/DeFinanceDbContext.cs` — new `DbSet<T>` properties

2. **Derive a migration name** — compose a PascalCase name that describes the change, e.g.:
   - New entity → `Add<EntityName>`
   - New column → `Add<ColumnName>To<TableName>`
   - Removed column → `Remove<ColumnName>From<TableName>`
   - New index or constraint → `Add<IndexName>IndexTo<TableName>`
   - Multiple changes → combine, e.g. `AddCurrencyAndExchangeRate`

3. **Run the migration command** from the solution root:
   ```
   .claude/scripts/migration-add.ps1 -Name <MigrationName>
   ```

4. **Verify** — confirm the generated files appear under
   `src/DeFinance.Infrastructure/Persistence/Migrations/` and that the `Up()` method matches the expected schema changes.

5. **Apply to database** — run:
   ```
   .claude/scripts/migration-update.ps1
   ```
   If the command fails (e.g. Postgres is not running), report the error clearly and tell the user to start the database with `docker compose up -d` then retry.

6. **Report** — summarise what was detected, the migration name chosen, the files created, and whether the database was updated successfully.

## Notes

- If no schema-affecting changes are detected, say so rather than creating an empty migration.
- The startup project is always `src/DeFinance.Api` and the migrations output dir is always `Persistence/Migrations` inside the Infrastructure project.
