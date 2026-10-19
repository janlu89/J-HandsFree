# Coding standards

These rules apply to every line of production and test code. They follow Robert C. Martin's Clean Code and Clean Architecture, the Microsoft .NET naming and design guidelines, and common TypeScript practice. A pull request that breaks a rule is changed before it is merged, or the rule is changed first through an ADR.

## 1. Principles

- **Readable first.** Code is read far more often than it is written. Optimise for the next reader.
- **Single Responsibility.** A class has one reason to change. A function does one thing.
- **SOLID** for every class that holds behaviour.
- **Dependency rule.** Source code dependencies point inwards: Mobile → Infrastructure.Android → Infrastructure.PlayerProtocol → UseCases → Domain. Inner layers never mention outer ones.
- **YAGNI and KISS.** Build what the current roadmap step needs, in the simplest form that keeps the rules above.
- **Leave it cleaner.** Every change may include a small improvement to the code it touches, never an unrelated rewrite.

## 2. Naming

| Element | Rule | Example |
| --- | --- | --- |
| Classes, records, structs | Nouns, PascalCase, say what the thing is | `PlaybackSession`, `SkipPolicy` |
| Interfaces | `I` prefix, describe a role | `IPlayerCommands`, `ISettingsStore` |
| Methods | Verbs or verb phrases | `MarkAdSkippable`, `SendAsync` |
| Async methods | End with `Async` | `AddAsync` |
| Booleans | Read as a question | `IsEnabled`, `HasActiveAd`, `CanSkip` |
| Events (things that happened) | Past tense | `AdStarted`, `AdBecameSkippable` |
| Commands (things to do) | Imperative | `SkipAd`, `PausePlayback` |
| Private fields | `_camelCase` | `_skipPolicy` |
| Locals and parameters | camelCase, full words | `videoId`, not `vid` |
| Constants | PascalCase | `DefaultPauseTimeout` |
| Test methods | `Unit_Scenario_ExpectedOutcome`, without the `Async` suffix even when async: only the test framework calls them | `Skip_WhenPolicyDisabled_SendsNoCommand` |

Additional rules:

- Names reveal intent. If a name needs a comment, rename it.
- No abbreviations except universally known ones (`Id`, `Url`, `Json`).
- No vague suffixes such as `Manager`, `Helper`, `Utils`, `Processor` or `Data`. Find the real responsibility and name it.
- One word per concept across the codebase: if it is `Send` in one port, it is not `Dispatch` in another.
- Names from the problem domain (playback, ad break, skip) win over technical names.

## 3. Functions

- **Small.** Aim for under 15 lines; more than 25 needs a reason in the pull request.
- **Do one thing.** If you can extract a meaningfully named function from it, it does more than one thing.
- **One level of abstraction per function.** A function either orchestrates steps or performs a low-level detail, never both. Read top to bottom like a narrative: each function is followed by the functions it calls (the step-down rule).
- **Few arguments.** Zero to two is ideal; three is the maximum. More than three means a missing type. The limit applies to methods and to the constructors of values. A constructor that receives injected dependencies takes one per collaborator; more than six suggests the class does too much.
- **No flag arguments.** `Skip(true)` hides two functions. Write `SkipNow()` and `ScheduleSkip()`.
- **Command-query separation.** A method either changes state or returns information, not both.
- **No hidden side effects.** A method named `GetSkipPolicy` never writes anything.
- **Guard clauses** instead of nested `if`. Nesting deeper than two levels is extracted.
- **No output parameters.** Return a value or a result type.

## 4. Classes and types

- **Small and cohesive.** Most methods use most fields. If a group of fields is used only by some methods, it is a separate class.
- **`sealed` by default.** Inheritance is a deliberate decision.
- **Composition over inheritance.**
- **Immutable value objects** as `record` or `readonly record struct`, validated in the constructor or a factory method.
- **Encapsulated entities.** No public setters on entities. State changes through methods that enforce invariants, for example `PlaybackSession.MarkAdSkippable()`.
- **Law of Demeter.** Talk to direct collaborators only. `session.CurrentAd.Kind.IsSkippable()` becomes `session.HasSkippableAd`.
- **One type per file**, file named after the type.

## 5. Layers and abstraction levels

| Layer | May reference | Must not contain |
| --- | --- | --- |
| Domain | The base class library only | Android, MAUI, JSON, logging, DI, I/O, time sources other than values passed in |
| UseCases | Domain, `Microsoft.Extensions.*.Abstractions`, `Mediator.Abstractions`, `TimeProvider` | Android, MAUI, JSON, concrete storage, `Mediator.SourceGenerator` |
| Infrastructure.PlayerProtocol | UseCases, `System.Text.Json` | Android, MAUI |
| Infrastructure.Android | UseCases, PlayerProtocol, AndroidX bindings | Business rules, MAUI pages |
| Mobile | Everything | Business rules |

- **Ports belong to the use cases.** Interfaces are defined by the layer that needs them, in its own words, not by the adapter that implements them.
- **Adapters are thin.** They translate between the outside world and the ports. A decision in an adapter is a bug.
- **Data crosses boundaries as simple types.** JSON DTOs never leave the protocol project; Android types never leave the Android project.
- **The composition root wires everything.** Only `MauiProgram` and the `Add...()` extension methods know concrete types.

## 6. Error handling

- Invariant violations and programmer errors throw exceptions (`ArgumentException`, `InvalidOperationException`).
- Expected outcomes are modelled as values, for example an invalid state transition returns a result instead of throwing during normal playback.
- Never swallow an exception silently. Catch it at a boundary, log it with context, and continue or report.
- Nothing ever throws into a WebView callback or an Android system callback. Adapters catch at that boundary.
- Never return `null` from a method that returns a collection; return an empty one.

## 7. Comments

- The code explains *what* and *how*. A comment may explain *why* when the reason is not visible in the code, for example a workaround for a platform bug with a link to the issue.
- No commented-out code.
- No `TODO`, `FIXME` or `HACK` comments. Open a GitHub issue instead.
- No comments that repeat the code, no section banners, no change logs in files.
- XML documentation only on types and members whose purpose is not obvious from the name.

## 8. C# rules

- Nullable reference types are enabled everywhere. No `!` null-forgiving operator without a comment explaining why it is safe.
- File-scoped namespaces; `using` directives outside the namespace.
- Primary constructors for dependency injection.
- `async` all the way. No `.Result`, no `.Wait()`, no `async void` except UI event handlers.
- Every async method that can be cancelled accepts a `CancellationToken` as its last parameter and passes it on.
- `ConfigureAwait(false)` in Domain, UseCases and Infrastructure; never in Mobile, where continuations may touch the UI.
- Time comes from `TimeProvider`, never from `DateTime.Now` or `DateTimeOffset.UtcNow` directly.
- Logging through `ILogger<T>` with message templates (`"Ad skipped after {Seconds}s"`), never string interpolation. Hot paths use the `LoggerMessage` source generator.
- `System.Text.Json` with source-generated contexts; no reflection-based serialization.
- No static mutable state. No service locator.
- Collection expressions, pattern matching and switch expressions where they make the code clearer.

## 9. MVVM rules (Mobile)

- View models use CommunityToolkit.Mvvm: `[ObservableProperty]` and `[RelayCommand]`.
- Code-behind contains view wiring only, such as connecting the native WebView to the bridge. No logic.
- View models depend on use case interfaces, never on adapters or Android types.
- XAML uses compiled bindings with `x:DataType`.
- One view model per page.

## 10. Dependency injection

- Constructor injection only.
- One extension method per layer: `AddUseCases()`, `AddPlayerProtocol()`, `AddAndroidInfrastructure()`, `AddMessaging()`, `AddPresentation()`.
- `AddMediator` is called only inside `AddMessaging()` in Mobile. `Mediator.SourceGenerator` is referenced only by Mobile and by the test project that checks the wiring.
- Lifetimes: coordinators, the bridge and stores are singletons; pages and view models are transient.

### Messages and handlers (Mediator)

- Messages are `sealed record` types in UseCases, grouped by feature folder, one message per file.
- Notifications describe something that happened, in the past tense: `AdBecameSkippable`. Commands ask for something, in the imperative: `UpdateSkipPolicy`. Queries start with `Get`: `GetSkipTotals`.
- A command or query has exactly one handler. A notification has one or more.
- A message is added in the same change as its first handler: the source generator reports a message without a handler (MSG0005), and CI treats warnings as errors.
- Handlers stay thin: they translate the message into calls on the Domain and on ports. Rules live in the Domain.
- Pipeline behaviours and the notification publisher contain cross-cutting concerns only (logging, failure isolation), never business rules.
- The mediator is for in-process dispatch from the edges (bridge, system media session, view models) to the use cases. Handlers do not send messages to other handlers to chain logic; shared logic belongs in the Domain or in a small injected collaborator.
- Calls that leave the process go through ports, never through the mediator.
- Unit tests call `Handle` directly. Only the composition test uses the generated mediator.

## 11. TypeScript rules (player scripts)

- `strict` mode, no `any`, no non-null assertions.
- Scripts observe and act; they never decide. Decisions belong to C#.
- Every CSS selector lives in `selectors.ts`.
- Messages are typed as discriminated unions that mirror the C# contract.
- No network calls, no `eval`, no dynamic script loading.
- Small pure functions; DOM access isolated in thin wrappers so logic can be tested with jsdom.
- ESLint and Prettier must pass.

## 12. Formatting

- `.editorconfig` is the source of truth. `dotnet format --verify-no-changes` and `npm run lint` run in CI.
- Member order inside a class: constants, fields, constructors, properties, public methods, private methods in step-down order.

## 13. Review checklist

- [ ] Names reveal intent and follow section 2.
- [ ] Every function does one thing at one level of abstraction.
- [ ] No dependency points outwards.
- [ ] New behaviour has tests that fail without the change.
- [ ] No `TODO`, commented-out code or debug leftovers.
- [ ] Public behaviour changes are reflected in the docs and the changelog.
