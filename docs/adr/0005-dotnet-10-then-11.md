# ADR 0005: Start on .NET 10, migrate to .NET 11 after release

- **Status:** Accepted
- **Date:** 2026-10-16

## Context

- .NET 10 is the current Long Term Support release, with stable .NET MAUI 10 servicing releases.
- .NET 11 is at release candidate stage and is scheduled to launch at .NET Conf 2026, November 10 to 12.
- .NET 11 changes the Android runtime default from Mono to CoreCLR, drops Android x86, and requires CommunityToolkit.Maui 15 or later.

## Decision

Build on .NET 10 (`net10.0` and `net10.0-android`). Migrate to .NET 11 as a dedicated roadmap phase after the general availability release.

Use CommunityToolkit.Maui 15 or later from the start, because earlier versions are not compatible with .NET 11.

## Consequences

- Development starts on a stable, supported toolchain.
- The migration is a planned, reviewable change with its own branch and tests, instead of a surprise during feature work.
- The migration must verify the CoreCLR runtime on a real device, the Android SDK level, the JDK version and the Media3 and WebKit bindings.
