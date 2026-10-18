# ADR 0013: Press Skip with a native tap; the page only locates the button

- **Status:** Accepted
- **Date:** 2026-10-18
- **Supersedes:** the script click as the way to press Skip in [ARCHITECTURE.md, sections 9 to 11](../architecture/ARCHITECTURE.md#9-player-scripts), and the ad messages of the contract under [ADR 0007](0007-typescript-player-scripts-and-message-contract.md): `ad.skip` with its `mode`, the `kind` of `ad.started` and the `skipped` flag of `ad.ended`.

## Context

The architecture planned a script click on the Skip button, with a native tap as the fallback if the page ignored script clicks. The [feasibility spike](../spikes/0001-feasibility.md#ads-step-06) measured both on real ads:

- `button.click()` from the injected script was ignored: the button stayed for 7.3 s more and the ad played to its end. Events raised by scripts carry `isTrusted: false`, and YouTube ignores them.
- A `MotionEvent` pair, down and up 60 ms apart, dispatched with `WebView.DispatchTouchEvent` at the centre of the button arrives as a trusted touch. It closed the ad 20 times out of 20, with a median of 211 ms and a maximum of 354 ms from the button appearing to the ad ending. 17 of the 19 everyday taps happened while the app was in the background with another app in front.
- `ad.started` could never tell whether a break would be skippable: its `kind` was `unknown` every time.
- The first spike journal let the page report `skipped: true` whenever a skip was requested, even when the page ignored it. Whether a skip worked is a decision, and decisions belong in C#.
- Breaks of two ads are reported by the player as one break.

## Decision

- The app presses Skip with a native tap. Script clicks are not used.
- `AdSkipCoordinator` decides to skip and sends `LocateSkipButton` through `IPlayerCommands`. The bridge sends `ad.locateSkip` to the page, which answers `ad.skipTarget` with the button rectangle in CSS pixels and the page's `devicePixelRatio`. The bridge publishes `SkipTargetLocated`; the coordinator sends `TapSkipButton` with that rectangle, and the bridge converts it to physical pixels and dispatches the touch on the UI thread.
- Message contract changes, before any release, so the protocol number stays 1:
  - `ad.skip` with `mode` is replaced by `ad.locateSkip`, without payload.
  - `ad.skipTarget` adds `devicePixelRatio`.
  - `ad.started` and `ad.ended` carry no payload.
- `AdSkipCoordinator` records a skip when the break ends within one second of the tap.
- A break is one `AdBreak`, whatever the number of ads in it. It starts as `AdKind.Unknown`, becomes skippable when the Skip button appears, and counts as non-skippable if it ends without one.

## Consequences

- Skip goes through the same path as a finger, which YouTube accepts, instead of relying on events the browser marks as untrusted.
- The page observes and reports; it never decides. The rule for a successful skip is plain C#, tested with `FakeTimeProvider`.
- The tap lands where the page measured the button about 40 ms earlier. If the layout moved in between, the tap could miss or hit another element, so the coordinator taps only a rectangle reported for the current break.
- A tap with the screen off was not tested in the spike; the field test in roadmap step 4.7 covers it.
- Roadmap steps 2.6, 5.2 and 5.5 follow this decision.
