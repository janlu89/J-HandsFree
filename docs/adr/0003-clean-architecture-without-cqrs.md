# ADR 0003: Clean Architecture with ports and adapters, without CQRS or a mediator

- **Status:** Accepted. The decision "no mediator library" is superseded by [ADR 0011](0011-source-generated-mediator.md).
- **Date:** 2026-10-16

## Context

The project is a single-user Android app. Its logic is reactive: the YouTube page emits events (an ad started, the Skip button appeared, playback paused) and the app answers with commands (skip, pause, next). Persistent data is small: settings and skip statistics.

Three styles were considered:

| Style | Fit for this app |
| --- | --- |
| CQRS (separate command and query models, often separate stores) | Solves problems this app does not have: read and write models that diverge, read-heavy scaling, complex reporting. Here it would add ceremony around a handful of events and one small table. |
| Mediator pipeline (MediatR or similar) | Adds indirection between a caller and a method it could call directly. MediatR 13 and later is dual-licensed (RPL 1.5 or commercial). The app has no cross-cutting pipeline behaviors that would justify it. |
| Clean Architecture with ports and adapters | Matches the problem: rules in the centre, YouTube page and Android services at the edge, behind interfaces. |

## Decision

- Use **Clean Architecture** with four circles: Domain, Use cases, Adapters (Infrastructure.Android), and the composition root with UI (Mobile).
- Use **ports and adapters**: the Use cases layer owns the interfaces for everything outside the process boundary.
- Use **MVVM** with CommunityToolkit.Mvvm in the UI.
- Model playback as an explicit **state machine** in the Domain.
- Use the **command pattern** for messages sent to the page (`PlayerCommand` records). This is a design pattern, not CQRS.
- Use cases are plain classes injected through interfaces. No mediator library.

## Consequences

- Rules are testable without Android: the Domain and Use cases projects target plain `net10.0`.
- When YouTube changes its markup, only a script or the bridge adapter changes.
- Replacing the WebView with a different player in the future would change one adapter and leave the Domain and Use cases untouched.
- If the app ever grows a remote backend with separate read models, CQRS can be reconsidered in a new ADR.
