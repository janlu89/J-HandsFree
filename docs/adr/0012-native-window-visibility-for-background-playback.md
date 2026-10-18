# ADR 0012: Keep background playback alive with a native window visibility override

- **Status:** Accepted
- **Date:** 2026-10-18
- **Supersedes:** the `visibility-shield` page script planned in [ARCHITECTURE.md, section 9](../architecture/ARCHITECTURE.md#9-player-scripts) and named in [ADR 0002](0002-host-youtube-mobile-site-in-webview.md).

## Context

The plan assumed that YouTube pauses because the page learns that it is hidden, through `document.hidden` and `visibilitychange`, and that a script injected at document start could hide that from the page.

The [feasibility spike](../spikes/0001-feasibility.md#background-playback-steps-03-and-04) showed otherwise:

- With the script alone, playback paused 1 to 13 ms after the app went to the background, in every run, although the page never received a `visibilitychange`.
- With no script and no native change, playback stopped within 5 s, the page logged no `pause` event, and playback resumed by itself when the app came back.
- The pause comes from Chromium inside the WebView. When the app goes to the background, Android reports the WebView's window as `Gone`, and Chromium suspends the page's media.
- A `MauiWebView` subclass that overrides `OnWindowVisibilityChanged` and always reports `Visible` kept playback running with the launcher, Waze and Google Maps in front. The page never saw itself hidden, so the script had nothing left to do.
- The same override keeps the page running at full rate in the background, also when playback is paused. A cached process doing so was killed by Android's excessive CPU check 15.6 minutes after the foreground service stopped.

## Decision

- `KeepVisibleWebView`, a subclass of MAUI's `MauiWebView`, overrides `OnWindowVisibilityChanged` and reports `Visible` to Chromium while background playback is active. A custom `WebViewHandler` creates it for the player page.
- Both types live in JHandsFree.Mobile, because they derive from MAUI types and Infrastructure.Android must not reference MAUI.
- The override is switched by a port, `IPlayerVisibility`, with `KeepVisible()` and `AllowHidden()`. `PlaybackCoordinator` keeps the page visible when background playback starts and allows it to be hidden when background playback is released after the pause timeout, so Chromium throttles a paused page. The adapter lives in Mobile next to the view it controls.
- The `visibility-shield` script is not built.

## Consequences

- Background playback depends on one platform override at the source of the signal, instead of a script racing the page's own event handlers. A change in YouTube's page code cannot undo it.
- Chromium keeps running the page while the app is in the background, which costs CPU, battery and memory. The foreground service keeps the process out of Android's background CPU check; the battery cost is measured in the field test. See risk R-16.
- The override relies on how Chromium reads window visibility. An Android System WebView update could change that, so the background check is repeated after major WebView updates.
- `IPlayerVisibility` is the one port whose adapter lives in Mobile rather than Infrastructure.Android.
- Roadmap step 4.1 builds `KeepVisibleWebView` instead of the script, and step 4.6 switches the override off when background playback is released.
