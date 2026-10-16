# ADR 0010: Test libraries

- **Status:** Accepted
- **Date:** 2026-10-16

## Context

The project is MIT licensed and public. Test dependencies should have permissive licenses and no telemetry.

- FluentAssertions 8 and later is distributed under a commercial license.
- Moq had a controversy over a bundled dependency that collected data at build time.

## Decision

| Need | Library | License |
| --- | --- | --- |
| Test framework | xUnit v3 | Apache 2.0 |
| Assertions | Shouldly | BSD 3-Clause |
| Test doubles | NSubstitute | BSD 3-Clause |
| Script tests | Vitest with jsdom | MIT |

## Consequences

- All test dependencies are compatible with the MIT license of the project.
- Assertion messages from Shouldly name the failing expression, which keeps test failures readable.
