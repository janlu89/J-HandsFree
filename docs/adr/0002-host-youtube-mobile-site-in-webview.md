# ADR 0002: Host the YouTube mobile website in a WebView

- **Status:** Accepted. Validated by the [phase 0 spike](../spikes/0001-feasibility.md): sign-in works with the default WebView user agent. The visibility shield it mentions is replaced by [ADR 0012](0012-native-window-visibility-for-background-playback.md).
- **Date:** 2026-10-16

## Context

The app has two core requirements:

1. Audio continues with the screen off or with another app in the foreground.
2. Skippable ads are skipped automatically, without touching the screen.

There are two ways to build a YouTube client:

| Option | How it works | Background playback | Ads |
| --- | --- | --- | --- |
| A. WebView | Load `m.youtube.com` in an Android WebView and inject small scripts | Hard: the page pauses itself when hidden; needs a visibility shield and a foreground service | Real ads are shown; skippable ones can be skipped by pressing the player's own Skip button |
| B. Stream extraction | Parse YouTube's internal API and play streams in a native player | Native and robust | No ads at all, which makes the app an ad blocker |

Option B contradicts the intent of requirement 2: the owner explicitly does not want an ad blocker. Option B also depends on undocumented internal APIs that change often.

## Decision

Use option A. The app hosts the official YouTube mobile website in a WebView. It keeps the user's signed-in session, personal playlists and subscriptions, and it presses Skip only when YouTube offers it.

## Consequences

- Ads still play. Non-skippable ads cannot be shortened.
- The app depends on YouTube's page markup. All selectors live in one module and can be refreshed as data, see [ADR 0007](0007-typescript-player-scripts-and-message-contract.md).
- Google may block sign-in inside an embedded WebView. If the spike confirms the block, the fallback is to play personal playlists set to *unlisted* through their links, without signing in.
- Automating interaction with YouTube may conflict with its Terms of Service. The app is for personal use and is not published on any store.
