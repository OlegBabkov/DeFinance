---
name: create-pull-request
description: Analyse commits on the current branch, draft a pull request title and description, ask the user for permission, then open the PR on GitHub.
allowed-tools:
  - Read
  - Grep
  - Glob
  - Bash
---

# Create Pull Request

Inspect all commits on the current branch since it diverged from the base branch, compose a clear PR title and description, **ask the user for explicit permission**, then create the PR.

## Step 1 — Gather branch context

```
git rev-parse --abbrev-ref HEAD          # current branch name
git log main..HEAD --oneline             # commits ahead of main
git diff main...HEAD --stat              # files changed vs main
```

Identify:
- The feature or fix being delivered (from branch name + commit subjects)
- Which layers were touched (Domain / Application / Infrastructure / Api / Tests)
- Whether this is a feature, fix, refactor, test, or chore

## Step 2 — Draft PR title and description

**Title rules:**
- Imperative mood, under 70 characters
- No type prefix (the PR description carries that detail)
- Example: `Add currency seeding on application startup`

**Description template:**

```
## Summary
- <bullet 1: what this PR does>
- <bullet 2: why / motivation if non-obvious>
- <bullet 3: any notable design decision>

## Changes by layer
- **Domain**: <what changed, or omit if nothing>
- **Application**: <what changed, or omit if nothing>
- **Infrastructure**: <what changed, or omit if nothing>
- **Api**: <what changed, or omit if nothing>
- **Tests**: <what changed, or omit if nothing>

## Test plan
- [ ] All existing tests pass (`dotnet test`)
- [ ] <specific scenario to verify manually if relevant>

🤖 Generated with [Claude Code](https://claude.ai/claude-code)
```

Omit any "Changes by layer" row that has no changes.

## Step 3 — Ask for permission

**ALWAYS stop here and show the user:**
1. The drafted PR title
2. The full description
3. The base branch (default: `main`)

Then ask:
> "Shall I create this pull request? You can approve as-is, ask me to adjust the title or description, or cancel."

Do **not** proceed until the user explicitly confirms. If they request changes, revise and ask again.

## Step 4 — Create the PR

Once the user confirms, run:

```
.claude/scripts/git-pr.ps1 -Title "<title>" -Body "<description>" -Base main
```

To open as a draft instead:
```
.claude/scripts/git-pr.ps1 -Title "<title>" -Body "<description>" -Base main -Draft true
```

## Step 5 — Confirm

Report the PR URL returned by the command so the user can open it immediately.

## Notes

- Never create the PR without explicit user confirmation — this is a shared, visible action.
- If `gh` is not authenticated, tell the user to run `gh auth login` first.
- If the branch has no commits ahead of `main`, stop and say so — there is nothing to PR.
- If the remote branch does not exist yet, tell the user to push first (`/commit-and-push`) before running this skill.
- Default base branch is `main`. If the project uses a different default (e.g. `develop`), adjust accordingly.
