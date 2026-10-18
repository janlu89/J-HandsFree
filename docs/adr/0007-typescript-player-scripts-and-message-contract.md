# ADR 0007: TypeScript player scripts with a versioned message contract

- **Status:** Accepted. The ad skip messages of the contract are changed by [ADR 0013](0013-native-tap-to-press-skip.md).
- **Date:** 2026-10-16

## Context

The app must observe and operate the YouTube player from inside the page. The scripts run in a page the app does not control, which changes without notice.

Options for the scripts: inline JavaScript strings in C#, plain JavaScript files, or a typed TypeScript project.

Options for the channel: the legacy `addJavascriptInterface`, which exposes a native object to every frame and origin, or AndroidX WebKit `addWebMessageListener`, which accepts messages only from allowed origins and provides a reply channel.

## Decision

- Write the scripts in **TypeScript** in `src/player-scripts`, bundled with esbuild into one file and embedded as a resource in Infrastructure.Android.
- Keep every CSS selector in one `selectors` module. Selectors can also be refreshed from a versioned JSON file in this repository; this is data only.
- Inject the bundle at document start with `addDocumentStartJavaScript`, restricted to the allowed YouTube origins.
- Communicate through `addWebMessageListener` with an origin allowlist.
- Use one JSON envelope with a `protocol` number for all messages, documented in [ARCHITECTURE.md](../architecture/ARCHITECTURE.md#10-message-contract).
- Scripts observe and act; they never contain decisions. Decisions live in C#.

## Consequences

- The message contract is typed on both sides and testable.
- Scripts are unit tested with Vitest against saved snapshots of the player markup.
- The build needs Node.js in addition to the .NET SDK, locally and in CI.
- If the WebView on a device lacks the required WebKit features, the app shows a clear message asking to update Android System WebView.
