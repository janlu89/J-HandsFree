# ADR 0001: Record architecture decisions

- **Status:** Accepted
- **Date:** 2026-10-16

## Context

Decisions made early in a project are easy to forget and hard to explain later. A new reader, or the author six months from now, needs to know not only what was chosen but also what was rejected and why.

## Decision

Significant architecture decisions are recorded as Architecture Decision Records in `docs/adr`, using the format proposed by Michael Nygard: context, decision, consequences.

- One decision per file, numbered sequentially: `NNNN-short-title.md`.
- A record is never edited after it is accepted, except to change its status. A new decision that replaces an old one is a new record, and the old one is marked **Superseded by ADR NNNN**.
- A pull request that changes an architectural rule must include or update an ADR.

## Consequences

- The reasoning behind the design is reviewable in the repository history.
- Writing a record takes a few minutes per decision.
