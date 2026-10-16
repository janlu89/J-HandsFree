# ADR 0008: Branching and merge strategy

- **Status:** Accepted
- **Date:** 2026-10-16

## Context

The repository is public. `master` holds what is released. `dev` is the integration and test branch. Work happens in short-lived `feature/*` and `fix/*` branches.

Squash merging keeps history readable: one commit per change, written as a Conventional Commit. However, squash merging a long-lived branch into another long-lived branch causes a known problem: the squashed commit on `master` does not exist on `dev`, so the two branches diverge, and every later `dev` to `master` pull request shows old changes again or conflicts.

## Decision

| Merge | Method | Why |
| --- | --- | --- |
| `feature/*` or `fix/*` into `dev` | **Squash** | One clean Conventional Commit per change |
| `dev` into `master` | **Merge commit** | `master` and `dev` keep a shared history; the merge commit marks the release |

Additional rules:

- `master` accepts pull requests only from `dev`. A required status check fails any other source branch.
- No direct pushes and no force pushes to `master` or `dev`. Both branches cannot be deleted.
- Rebase merging is disabled.
- Branch protections are configured with GitHub repository rulesets, including the allowed merge method per branch.

Full procedure: [GIT_WORKFLOW.md](../GIT_WORKFLOW.md).

## Alternative considered

Squash everywhere, then merge `master` back into `dev` after every release. It works but adds a sync commit and a manual step to every release. Rejected for that reason.

## Consequences

- `dev` history is one commit per feature or fix.
- `master` history is one merge commit per release, each pointing to a tagged, signed APK.
