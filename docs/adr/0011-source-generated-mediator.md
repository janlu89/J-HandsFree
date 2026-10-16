# ADR 0011: Dispatch messages with the source-generated Mediator library

- **Status:** Accepted
- **Date:** 2026-10-16
- **Supersedes:** the decision "no mediator library" in [ADR 0003](0003-clean-architecture-without-cqrs.md). The rest of ADR 0003, including the rejection of CQRS, still applies.

## Context

ADR 0003 rejected a mediator because no cross-cutting behaviour seemed to justify it. A closer look at the message flow shows three real needs:

1. **Fan-out.** One player event, such as "the Skip button appeared", must reach several coordinators.
2. **A single boundary for failures and logging.** Every event and command should be logged in one place, and an exception in one handler must never reach the WebView or stop the other handlers.
3. **Decoupled callers.** View models and the system media session send commands without knowing which coordinator handles them.

Options considered:

| Option | License | Assessment |
| --- | --- | --- |
| MediatR 12.5.0, last Apache 2.0 version | Apache 2.0 | Frozen: no security fixes or support. Reflection-based handler discovery is fragile under Android trimming. |
| MediatR 13 and later | RPL 1.5 or commercial, free Community tier with a license key | Maintained, but a differently licensed dependency in an MIT repository, and warnings without a key. |
| Mediator by Martin Othamar | MIT | Same model as MediatR (requests, commands, queries, notifications, pipeline behaviours). Handlers and registrations are generated at compile time: no reflection, Native AOT and trimming safe. |
| Hand-written mediator | None | Possible, but pipelines and notification publishing would be maintained here. |

## Decision

Use **Mediator 3.0.2** (`Mediator.Abstractions` and `Mediator.SourceGenerator`), pinned in `Directory.Packages.props`.

| Concern | Rule |
| --- | --- |
| Package placement | `Mediator.Abstractions` in UseCases. `Mediator.SourceGenerator` only in the executable projects: Mobile, and the test project that verifies the wiring. |
| Registration | `AddMediator` is called once, in Mobile, inside `AddPresentation()` or a dedicated `AddMessaging()` extension. Scanning is limited to the UseCases assembly. |
| Lifetime | `Singleton` (the library default). Coordinators keep playback state for the whole process. |
| Player events | `INotification` records, named in the past tense, published by the bridge through `IPublisher`. |
| Actions | `ICommand` records, named in the imperative, sent by view models and the system media session. |
| Reads | `IQuery<T>` records, named `Get...`, sent by view models. |
| Failures in notifications | A custom `INotificationPublisher`, `IsolatingNotificationPublisher`, runs handlers one by one, logs each failure with context and continues with the next handler. Nothing propagates to the caller. |
| Logging of commands and queries | One pipeline behaviour, `LoggingBehavior`, registered through `MediatorOptions.PipelineBehaviors`. |
| Outbound calls | Calls that leave the process boundary stay behind ports (`IPlayerCommands`, `ISystemMediaSession`, repositories). The mediator dispatches inside the application; it does not replace ports. |

This is not CQRS: there is still one model and one store. `ICommand` and `IQuery` are only message names.

## Consequences

- The `IPlayerEventSource` port from ADR 0003 is replaced by notifications. The bridge depends on `IPublisher` from `Mediator.Abstractions`.
- Coordinators become notification handlers. They are still plain classes, unit tested by calling `Handle` directly.
- A composition test in `JHandsFree.UseCases.Tests` references the source generator, builds a real service provider and checks that publishing an event reaches the right handler.
- Wiring errors appear at compile time instead of on the phone.
- Mediator 3.0.2 targets .NET Standard 2.0. Version 3.1, which lists .NET 10 explicitly, is a release candidate at the time of writing. Phase 1 confirms that 3.0.2 works on `net10.0`; the move to 3.1 happens through a normal dependency update once it is stable.
