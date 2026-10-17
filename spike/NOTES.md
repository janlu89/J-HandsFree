# Feasibility spike notes

Working log of the phase 0 spike. Step 0.8 turns it into `docs/spikes/0001-feasibility.md`.

## Test device

| Item | Value |
| --- | --- |
| Phone | Xiaomi Redmi Note 12 Pro+ 5G (model `22101316UG`, device `rubypro`) |
| Android | 14 (API 34), security patch 2026-08-01 |
| Vendor OS | Xiaomi HyperOS 2.0 (`OS2.0.16.0.UMOEUXM`) |
| CPU ABI | `arm64-v8a` |
| Android System WebView | 155.0.8059.43 |
| Chrome | 155.0.8059.79 |

## Build machine

| Item | Value |
| --- | --- |
| .NET SDK | 10.0.402 |
| Workload | `maui-android`, 10.0.400 band (`Microsoft.Android.Sdk` 36.1.118) |
| JDK | Microsoft OpenJDK 21.0.12 |
| Android SDK | platform `android-36`, build-tools 36.0.0 |

## Step 0.1: throwaway MAUI app

**Result: confirmed.** `spike/JHandsFree.Spike` targets `net10.0-android` only, builds with no warnings and runs on the phone. `MainActivity` is displayed 2.1 s after launch (Debug build, first start after install).

```powershell
dotnet build spike\JHandsFree.Spike -t:Run -f net10.0-android
```

### Findings

- JDK 21 works with .NET 10 for Android, so the local toolchain matches CI.
- HyperOS refuses USB installs until **Developer options → Install via USB** is enabled.
- The phone has more than one Android user. A restriction on a secondary user makes `adb install` fail for every user with `INSTALL_FAILED_USER_RESTRICTED`. Installing for the owner only works: `adb install --user 0`, or the MSBuild property `AndroidDeviceUserId=0`, now set in the spike project.
- The Activity is registered in Java as `crc64b20a05b7cba24e10.MainActivity`: .NET for Android names its Java wrappers after a hash of the namespace. Components that the system or other apps address by name, such as the Media3 service or launcher shortcuts, need a stable name through the `Name` property of `[Activity]` or `[Service]`.
- Logcat shows HyperOS framework errors in the app process at startup (`mi_exception_log`, `FramePredict`, `QT`, `LB`). They come from the vendor framework, not from the app, and can be filtered out when reading logs.

## Step 0.2: mobile site and Google sign-in

**Result: confirmed with the default WebView user agent.** `https://m.youtube.com` loads in the MAUI `WebView`, and Google sign-in completes without the "This browser or app may not be secure" block. After signing in, subscriptions and personal playlists are visible, which meets the acceptance criterion of F-03.

| User agent | Fresh sign-in | Existing session after switching to it |
| --- | --- | --- |
| `Default`: `... Build/UP1A.231005.007; wv) ... Version/4.0 Chrome/155.0.8059.43 Mobile Safari/537.36` | Succeeded twice, the second time after clearing cookies, storage and cache | Not applicable |
| `WithoutWebViewMarker`: `; wv` removed | Not attempted, because the default works | Still signed in |
| `WithoutWebViewAndVersionMarkers`: `; wv` and `Version/4.0` removed | Not attempted, because the default works | Still signed in |

Sign-in path, as logged without query strings:

1. `m.youtube.com/signin`
2. `accounts.google.com/ServiceLogin`
3. `accounts.google.com/v3/signin/identifier` (email, password and any second factor typed by the user)
4. `accounts.google.com/CheckCookie`
5. `accounts.youtube.com/accounts/SetSID`, then `accounts.google.<country>/accounts/SetSID`
6. back to `m.youtube.com`

On the first sign-in Google also showed `gds.google.com/web/recoveryoptions`, its usual prompt to add recovery options after a successful sign-in.

### Findings

- Risk R-01 does not occur on this device and WebView version, so the unlisted playlist fallback is not needed for now. Google can change this at any time; the two user agent variants are the first fallback to try if it does.
- The test ran with JavaScript, DOM storage and third-party cookies enabled. Whether sign-in also works without third-party cookies was not tested.
- The session is stored in the WebView cookies and survives a user agent change. Survival across an app restart was not tested in this step; step 0.7 covers it.
- Sign-in URLs can carry account hints and tokens in their query strings, so the spike logs only scheme, host and path.

## Step 0.3: playback with another app in front

**Result: confirmed with a native change; the page script alone is not enough.** With the screen on, playback continues while the launcher or Waze is in front, as long as the WebView keeps reporting its window as visible to Chromium. The JavaScript visibility shield alone does not prevent the pause.

### Setup

- `Xamarin.AndroidX.WebKit` 1.14.0.1. `DOCUMENT_START_SCRIPT` is supported on WebView 155, and both scripts are injected for `https://m.youtube.com` only.
- `media-probe.js`, always on, logs media `play`, `playing` and `pause` events and every `visibilitychange` that reaches the page.
- `visibility-shield.js`, switchable, makes `document.hidden` always `false` and `visibilityState` always `visible`, and swallows page-level `visibilitychange` and `blur` events.
- `KeepVisibleWebView`, switchable, is a `MauiWebView` subclass registered through a custom `WebViewHandler`. It overrides `OnWindowVisibilityChanged` and reports `Visible` to Chromium.
- Measurements: page console messages from logcat (tag `chromium`), the app's audio players from `dumpsys audio` every 5 s, and the process state from `dumpsys activity processes`.

### Runs

| Shield | Native | Background runs | What the page saw | Playback |
| --- | --- | --- | --- | --- |
| off | off | 1, lasting 3 s | `visibilitychange` with `document.hidden=true` | No pause within 3 s |
| on | off | 4 | Nothing: the shield swallowed `visibilitychange` | Paused 1 to 13 ms after the app went to the background, every time; resumed by itself when the app came back |
| on | on | 2, lasting 11 s and 12 s | Nothing: Chromium never saw the window as hidden | Continued, also with Waze in front; one pause when the YouTube app was opened |
| off | on | 1, lasting 10 s | Nothing | Continued; the audio player stayed `started` throughout |

### Findings

- Overriding `OnWindowVisibilityChanged` keeps playback running in the background. When the app goes to the background Android reports `Gone`, not `Invisible`; the override reports `Visible` instead, so Chromium never fires `visibilitychange` and the shield has nothing to do.
- With only the shield, the page never learned it was hidden, yet the video paused within milliseconds and resumed by itself on return. The 3 s control run of this step was too short to compare; the longer control run recorded in step 0.4 settles it.
- Opening the YouTube app paused playback: another media app takes the audio focus, which is expected Android behaviour. Waze in front did not pause it.
- Background runs lasted at most 12 s. The process moved to process states 15 and 16 (last and cached activity) and was never frozen in that time. Whether Android stops playback after minutes without a foreground service was not tested; step 0.4 covers it.
- The process restarted once during the test (new pid at 01:02:46); the cause was not recorded.
- Implementation notes: the binding exposes Java's `ScriptHandler` as `IScriptHandler`; `Xamarin.AndroidX.WebKit` 1.15 and later pull AndroidX Lifecycle and Collection versions outside the constraints of MAUI 10 (NU1608), while 1.14.0.1 does not; page `console` messages reach logcat under the tag `chromium`.

## Step 0.4: Media3 session service

**Result: confirmed for the session, the foreground service, the media controls and ducking; the 30-minute screen-off run is deferred to the field test.** The owner chose to run the long screen-off test in the car (roadmap step 4.7) instead of at the desk.

### Setup

- `Xamarin.AndroidX.Media3.Session` 1.11.1.
- `PlaybackService`: a `MediaSessionService` with the stable Java name `com.janlu89.jhandsfree.spike.PlaybackService`, foreground service type `mediaPlayback` and the `androidx.media3.session.MediaSessionService` intent filter.
- `WebViewSessionPlayer`: a `SimpleBasePlayer` facade exposing play and pause only. Play and pause run `video.play()` and `video.pause()` in the page, and a partial wake lock is held while it plays.
- Manifest permissions: `FOREGROUND_SERVICE`, `FOREGROUND_SERVICE_MEDIA_PLAYBACK`, `POST_NOTIFICATIONS`, `WAKE_LOCK`.
- The page connects a `MediaController` to the service and sends play, the same path the real UI will use.
- Native keep-visible override on, shield off. The phone stayed connected over USB, so it was charging and Doze did not apply.

### Control run for step 0.3

With shield, native override and session all off, the app went to the background for 58 s with the screen on. The audio player stopped within 5 s, the video position stayed at 566.7 s for the whole time, and playback resumed by itself when the app came back. The page logged no `pause` event.

Together with step 0.3, this shows that playback stops whenever Chromium considers the WebView hidden, whether or not the page is told. The missing `pause` event and the automatic resume are consistent with a suspension inside the WebView rather than a decision of the page's own code. Only the native visibility override prevents it.

### Session run

| Check | Result |
| --- | --- |
| Foreground service | Started as soon as the session played; the process moved to process state 4 (foreground service) in the background and was never frozen |
| Playback in the background | The audio player stayed `started` for the whole 1 min 53 s in the background, including with Google Maps navigating in front |
| Lock screen and notification controls | Four play and pause commands reached the page about 25 ms after each tap |
| Ducking during navigation prompts | Reported by the owner: the audio lowers and comes back. The 5 s sampling did not catch a prompt, which lasts 1 to 3 s |
| Seek bar in the notification | Does not work, as expected: the facade publishes neither duration nor position and does not offer a seek command |
| Screen off | At most a few seconds, never caught by the 5 s sampling; the long screen-off run is deferred to the field test |

### Findings

- Every `Xamarin.AndroidX.Media3.Session` release from 1.9 on needs AndroidX Lifecycle 2.10 or later, while packages pulled in by MAUI 10 pin the 2.9 series (NU1608). Nine explicit references align the set; the spike project lists them in a labelled item group. In the real app they belong in `Directory.Packages.props`, where transitive pinning is already enabled.
- AndroidX Lifecycle 2.11 requires API 23, so the minimum Android version rises from 5.0 (API 21) to 6.0 (API 23).
- In the .NET bindings, Media3's `Player` constants live in `BasePlayer.InterfaceConsts`, and every builder result is annotated as nullable.
- Guava's `Futures` is not exposed by the .NET binding, and AndroidX `ResolvableFuture` is marked as internal API. `CallbackToFutureAdapter` from AndroidX is the supported way to return a completed `ListenableFuture` to Media3.
- A denied notification permission does not stop the foreground service: on Android 13 and later, media session notifications are exempt from it.
- Pausing from the page's own player does not reach the session, which keeps reporting "playing" and keeps the foreground service. Page-to-app events (step 0.5) are needed to keep the session state true.
- Swiping the app away from the recent apps while the session was paused ended the process.

## Step 0.5: page bridge with an origin allowlist

**Result: confirmed.** `addWebMessageListener` injects the `jhf` object only into frames from `https://m.youtube.com`, messages round-trip in a few milliseconds, and the Media3 session now follows the page in both directions.

### Setup

- `WebViewCompat.AddWebMessageListener` with the object name `jhf` and the single origin rule `https://m.youtube.com`, registered before the first page load. `WEB_MESSAGE_LISTENER` is supported on WebView 155.
- `bridge-client.js`, injected at document start before the media probe, wraps `jhf.postMessage` in the envelope `{ protocol, type, sentAtMs, payload }` from the architecture, sends `script.ready`, and executes the `player.play` and `player.pause` commands.
- `media-probe.js` sends `player.state` (`playing`, `paused`, `ended`, with position, duration and video id) on the `/watch` page.
- `PageBridge` accepts messages only from the main frame of the allowed origin, keeps the latest `JavaScriptReplyProxy` for commands, and forwards `player.state` to the session player.
- The session player now sends play and pause to the page through the reply proxy instead of `EvaluateJavascript`.
- `bridge.ping` and `bridge.echo` are spike-only message types used to measure latency; they are not part of the contract.

### Results

| Check | Result |
| --- | --- |
| Page from a disallowed origin (`https://origin-check.invalid`, loaded with `LoadDataWithBaseURL`) | `typeof jhf` is `undefined` |
| `https://m.youtube.com` | `typeof jhf` is `object` |
| `script.ready`, page to app | 38 to 50 ms, including page start-up |
| Round trip, 20 sequential pings, two runs | Median 2 to 3 ms, minimum 1 ms, maximum 61 to 75 ms |
| `player.state`, page to app | 4 to 72 ms |
| Command from the notification to the page | 26 to 30 ms from the command to the video `pause` or `playing` event |
| Play and pause started inside the page | The session followed within 35 ms both times, so the notification shows the right state |

The page-initiated check ran through the WebView remote debugging protocol (`adb forward` to `webview_devtools_remote_<pid>`, then `Runtime.evaluate` of `video.play()` and `video.pause()`), which exercises the same code path as YouTube's own play button.

### Findings

- The origin allowlist is enforced by WebView itself, before any app code runs; the origin and main-frame checks in `PageBridge` are a second line of defence.
- The bridge is far from being a bottleneck: a median round trip of 2 to 3 ms leaves almost the whole one-second budget of F-02 to the page and the player.
- A reply proxy belongs to one page instance. A reload brings a new `script.ready` with a new proxy, so commands must always use the latest one.
- After a pause, Media3 keeps the service in the foreground for `MediaSessionService.DefaultForegroundServiceTimeoutMs`, 600,000 ms (10 minutes). This matches the default of `PauseTimeoutPolicy`, which can be implemented with `SetForegroundServiceTimeoutMs`.
- The Debug build exposes the WebView remote debugging socket. It is a useful test tool for the ad steps, and it must stay disabled in Release builds.
- The notification seek bar still stays at zero and cannot be dragged, as recorded in step 0.4. The page now sends position and duration, so the facade can publish them; seeking still needs an absolute seek command.

## Step 0.6: detecting ads and pressing Skip

**Result so far: a script click is ignored, a native tap works.** Risk R-05 occurred and its planned fallback resolves it. Two real ads have been recorded; the roadmap asks for at least 20, so collection continues in everyday use with the native tap as the default.

### Setup

- `ad-watcher.js`, injected at document start, observes `#movie_player` with a `MutationObserver` limited to the `class`, `style`, `hidden`, `aria-hidden` and `disabled` attributes, with no polling. An ad is showing while the player has the class `ad-showing`; the Skip button is the first visible, enabled `button` or `[role="button"]` whose class contains `skip`.
- The page only reports `ad.started`, `ad.skippable` and `ad.ended`. The app decides and sends `ad.skip`, with mode `click` (the script calls `button.click()`) or `locate` (the script answers `ad.skipTarget` with the button rectangle in CSS pixels).
- For a native tap, the app multiplies the centre of the rectangle by `devicePixelRatio` (2.75 on this phone) and dispatches a `MotionEvent` pair, down then up 60 ms later, with source `Touchscreen`, through `WebView.DispatchTouchEvent`.
- Every event goes to `ad-journal.jsonl` in the app's private storage, and the player markup is saved when an ad starts and when it becomes skippable. The data stays on the phone and in a temporary folder on the development PC; it is not committed.
- The Skip button in the spike cycles through Off, Click and Tap and remembers the choice.

### Player markup observed

- Player: `div#movie_player.html5-video-player.ytp-mweb-player`, with the ad module `div.video-ads.ytp-ad-module` always present (player class `ad-created`).
- Skip button: `button.ytp-ad-skip-button-modern.ytp-button.ytp-ad-skip-button-icon-delhi`, text "Skip", about 67 by 32 CSS pixels, shown about 5.8 s after the ad starts.
- On the home page the player exists but is hidden, so markup checks must run on a `/watch` page.

### Real ads

| Time | Mode | Skip button shown after | Result |
| --- | --- | --- | --- |
| 02:00:37 | Click | 5.9 s | The app requested the skip after 3 ms and the script clicked after 12 ms, but the button stayed for another 7.3 s: the click was ignored. The ad lasted 13.1 s. |
| 02:04:18 | Tap | 5.8 s | Skip target received after 52 ms, tap at (944, 643) physical pixels, button gone 165 ms after it appeared, ad ended. |

### Findings

- YouTube ignores clicks raised by scripts (`isTrusted` is `false`). A touch dispatched to the WebView arrives as a trusted event and is accepted.
- The native tap path is fast: 165 ms from the Skip button appearing to the ad ending, well inside the one-second target of F-02.
- The `ad.skip` mode `locate` and the `ad.skipTarget` event already in the message contract are exactly what the fallback needs.
- The first journal version reported `skipped: true` whenever a skip was requested, even when the page ignored it. `ad.ended` now reports a skip only when the ad ends within 1 s of the request, and adds `msFromSkipRequestToEnd` and `offeredSkip`.
- Open: the 20-ad count, ad pods ("ad 1 of 2"), and taps while the screen is off. Collection continues in everyday use, including in the car.

## Implications for later steps

- The native tap is the primary skip method, not a fallback: `AdSkipCoordinator` should send `ad.skip` with mode `locate`, and the bridge adapter performs the tap. Script clicks can be dropped. ARCHITECTURE section 9 and the phase 0 report must say so.
- The notification seek bar needs the facade to publish duration and position and to handle seeking. The message contract offers only the relative `player.seekBy`; an absolute `player.seekTo` command is missing and must be added in step 0.8 or phase 3.
- The native window visibility override, not the page script, is what keeps playback alive. Section 9 of the architecture and ADR 0007 describe the visibility shield as a page script; step 0.8 must record the change in a new ADR, and `KeepVisibleWebView` becomes part of Infrastructure.Android.
- Sign-in works with the default user agent, so the real app keeps it unchanged. Changing the user agent is a fallback only, because it can also change the markup YouTube serves and break the selectors.
- The phone runs Android 14, not 17. The Android 17 background audio restrictions described in ADR 0006 cannot be observed on it; confirming them needs an Android 17 emulator or device. Starting playback from the visible UI stays the rule, because Android 14 already requires a typed foreground service.
- Xiaomi's battery manager is known to stop background apps (risk R-06). Steps 0.4 and 0.7 must record the HyperOS battery and autostart settings used for each test.
