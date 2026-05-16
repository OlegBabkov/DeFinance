---
name: commit-and-push
description: Inspect staged and unstaged changes, build a structured conventional commit message grouped by project layer, commit, and push to the remote branch.
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
---

# Commit and Push

Analyse all pending changes, compose a well-structured commit message following Conventional Commits, then commit and push.

## Step 1 — Understand the changes

Run the following to get a full picture:

```
git status
git diff --stat HEAD
```

Group changed files by layer:

| Layer | Paths |
|---|---|
| Domain | `src/DeFinance.Domain/` |
| Application | `src/DeFinance.Application/` |
| Infrastructure | `src/DeFinance.Infrastructure/` |
| Api | `src/DeFinance.Api/` |
| Tests | `tests/` |
| Config / Tooling | `docker-compose.yml`, `.claude/`, `*.csproj`, `*.slnx` |

## Step 2 — Choose commit type and scope

Use Conventional Commits:

| Type | When to use |
|---|---|
| `feat` | New entity, endpoint, feature, DTO, repository |
| `fix` | Bug fix, corrected mapping, wrong configuration |
| `refactor` | Restructured code, no behaviour change |
| `chore` | NuGet bumps, gitignore, docker, tooling, skills/scripts |
| `test` | Adding or updating tests |
| `docs` | README, comments, skill files |
| `migration` | New EF Core migration |

Scope is the primary layer or entity affected, e.g. `feat(currency)`, `migration(currency)`, `chore(docker)`.

When changes span multiple layers for the same feature, use the entity/feature name as scope and list all layers in the body.

## Step 3 — Build the commit message

Structure:
```
<type>(<scope>): <short imperative summary under 72 chars>

Changes by layer:
- Domain: <what changed>
- Application: <what changed>
- Infrastructure: <what changed>
- Api: <what changed>
- Tests: <what changed>

(omit layers with no changes)
```

Rules:
- Subject line: imperative mood, no period, under 72 characters
- Body: one bullet per layer, concrete — name the files or classes added/changed
- Omit layers that have no changes
- If only one layer changed, body is optional

## Step 4 — Commit

```
.claude/scripts/git-commit.ps1 -Message "<full commit message>"
```

To commit only specific files instead of all changes:
```
.claude/scripts/git-commit.ps1 -Message "<message>" -Files @("path/to/file1", "path/to/file2")
```

## Step 5 — Push

```
.claude/scripts/git-push.ps1
```

To push a specific branch:
```
.claude/scripts/git-push.ps1 -Branch <branch-name>
```

## Step 6 — Confirm

Report the commit hash, subject line, and the remote branch it was pushed to.

## Notes

- Never force-push (`git push --force`) without explicit user instruction.
- If `git push` is rejected because the remote has diverged, stop and tell the user — do not rebase or merge automatically.
- Do not include generated files (`bin/`, `obj/`, migration Designer files) unless they are genuinely part of the change being committed.
- If there is nothing to commit, say so and stop.
