# Spike 0001: feasibility on a real phone

- **Dates:** 16 to 18 October 2026
- **Code:** branch `spike/feasibility`, a throwaway MAUI app that is never merged
- **Status:** Complete. Every assumption in [ARCHITECTURE.md, section 13](../architecture/ARCHITECTURE.md#13-validated-by-the-feasibility-spike) has a result, and the rejected one has a working fallback.

## Summary

| # | Assumption | Result | Decision |
| --- | --- | --- | --- |
| 1 | Google sign-in works inside the WebView. | Confirmed with the default WebView user agent. | Keep the default user agent. The unlisted playlist fallback is not needed. |
| 2 | The WebView keeps playing audio in the background while the foreground service runs. | Confirmed with another app in front, for 67 minutes in one foreground session, but only with a native window visibility override; the page script alone does not work. The long screen-off run moves to the field test. | [ADR 0012](../adr/0012-native-window-visibility-for-background-playback.md) |
| 3 | `addDocumentStartJavaScript` and `addWebMessageListener` are available through the .NET AndroidX WebKit bindings. | Confirmed with `Xamarin.AndroidX.WebKit` 1.14.0.1 on WebView 155. | [ADR 0014](../adr/0014-androidx-alignment-and-android-6-minimum.md) for the version |
| 4 | Media3 `SimpleBasePlayer` can be subclassed from .NET and drive the media notification. | Confirmed with Media3 1.11.1, after aligning nine AndroidX packages. The minimum Android version rises to 6.0. | [ADR 0014](../adr/0014-androidx-alignment-and-android-6-minimum.md) |
| 5 | The Activity is not destroyed while backgrounded during a one-hour session. | Confirmed: never destroyed on its own, in background periods of up to 75 minutes. Without the foreground service, the whole process is killed instead. | The foreground service runs whenever audio plays. The WebView stays in the Activity. |
| 6 | The Skip button accepts a click dispatched by a script. | Rejected: YouTube ignores script clicks. The planned fallback, a native tap, skipped 20 ads out of 20. | [ADR 0013](../adr/0013-native-tap-to-press-skip.md) |

Phase 1 can start. The [open points](#open-points) are covered by later roadmap steps.

## Test device and toolchain

| Item | Value |
| --- | --- |
| Phone | Xiaomi Redmi Note 12 Pro+ 5G (model `22101316UG`) |
| Android | 14 (API 34), vendor OS Xiaomi HyperOS 2.0 (`OS2.0.16.0.UMOEUXM`), `arm64-v8a` |
| Android System WebView | 155.0.8059.43 |
| .NET | SDK 10.0.402, workload `maui-android` (`Microsoft.Android.Sdk` 36.1.118) |
| JDK and Android SDK | Microsoft OpenJDK 21.0.12, platform `android-36`, build-tools 36.0.0 |

## Method

- Each step added the smallest piece of code that could answer its question, then measured on the phone.
- Measurements came from the app log and the page console in `logcat`, the app's audio players from `dumpsys audio`, process states from `dumpsys activity`, and JavaScript run in the page through the WebView remote debugging protocol.
- Steps 0.6 and 0.7 ran for two days of everyday use, including car journeys. The app wrote a journal on the phone: ad events with timings, and process, Activity, WebView, service and screen events with a heartbeat every 5 minutes. Process exit reasons came from Android's `ApplicationExitInfo`.
- Device data, which contains video ids and player markup, stayed on the phone and the development machine. Screenshots were not taken, because a signed-in YouTube page shows the account and its history; the tables below are the evidence.

## Results

### Sign-in (step 0.2)

- Google sign-in completed twice with the default WebView user agent, the second time after clearing cookies, storage and cache. There was no "This browser or app may not be secure" block. Subscriptions and personal playlists were visible, which meets the acceptance criterion of F-03.
- Two user agent variants, without the `; wv` marker and without `Version/4.0`, kept the existing session. A fresh sign-in with them was not needed.
- The test ran with JavaScript, DOM storage and third-party cookies enabled.
- Sign-in URLs can carry account hints and tokens in their query strings, so the spike logged only scheme, host and path.

### Background playback (steps 0.3 and 0.4)

| Visibility shield script | Native override | What the page saw | Playback in the background |
| --- | --- | --- | --- |
| Off | Off | `visibilitychange` with `document.hidden=true` | Stopped within 5 s in a 58 s run, resumed by itself on return, no `pause` event |
| On | Off | Nothing: the shield swallowed `visibilitychange` | Paused 1 to 13 ms after the app went to the background, in 4 runs out of 4 |
| On | On | Nothing | Continued, also with Waze in front |
| Off | On | Nothing | Continued |

- Playback stops whenever Chromium considers the WebView hidden, whether or not the page is told. When the app goes to the background, Android reports the WebView's window as `Gone`. A `MauiWebView` subclass that overrides `OnWindowVisibilityChanged` and reports `Visible` keeps playback running, and the page never sees itself hidden.
- With the Media3 session, the process ran as a foreground service in the background and was never frozen. Lock screen and notification commands reached the page about 25 ms after each tap. The owner reported that navigation prompts lower the audio and that it comes back.
- In everyday use, one foreground session ran for 67 minutes during a car journey, with another app in front, without an interruption. The owner reports about three hours of car use on 17 October without a problem; the journal does not show which processes were in the car.
- After a pause, Media3 keeps the service in the foreground for 10 minutes (`DefaultForegroundServiceTimeoutMs`), which matches the default of `PauseTimeoutPolicy`.
- A denied notification permission does not stop the foreground service: on Android 13 and later, media session notifications are exempt from it.
- The notification seek bar stays at zero, because the player facade publishes neither duration nor position and offers no seek command. The owner decided not to build it: it would also need an absolute seek command in the message contract.
- The screen was off for a few seconds at most during these runs. The long screen-off run moves to the field test.

### Page bridge (step 0.5)

| Check | Result |
| --- | --- |
| Page from a disallowed origin (`https://origin-check.invalid`) | The bridge object `jhf` is `undefined` |
| `https://m.youtube.com` | `jhf` is an `object` |
| Round trip, 20 sequential pings, two runs | Median 2 to 3 ms, maximum 61 to 75 ms |
| `player.state`, page to app | 4 to 72 ms |
| Notification command to the video `pause` or `playing` event | 26 to 30 ms |
| Play and pause started inside the page | The session followed within 35 ms |

- WebView itself enforces the origin allowlist before any app code runs. The origin and main-frame checks in the app are a second line of defence.
- A reply proxy belongs to one page instance. A reload brings a new `script.ready` with a new proxy, so commands must always use the latest one.

### Ads (step 0.6)

The ad watcher observes the player with a `MutationObserver` limited to a few attributes, with no polling. An ad is showing while the player has the class `ad-showing`.

| Measure | Result |
| --- | --- |
| Script click (`button.click()`) | Ignored: the Skip button stayed for 7.3 s more and the ad played to its end |
| Ad breaks in everyday use, 17 and 18 October | 23 |
| Breaks with a Skip button | 19 in everyday use, 20 native taps in total with the first session |
| Confirmed skips by native tap | 20 of 20: the ad ended within 1 s of the tap every time |
| Skip button appearing to ad ending | Median 211 ms, maximum 354 ms |
| Where the time goes (medians) | Skip button reported to skip request 3 ms, request to button rectangle 27 ms, rectangle to tap 9 ms, tap to button gone 167 ms |
| Taps with the app in the background, another app in front and the screen on | 17 of 19 |
| Taps with the screen off | None: no break started while the screen was off |
| Breaks without a Skip button | 3, played to the end in 23 to 33 s; one more break has no recorded end |

- YouTube ignores clicks raised by scripts, which the browser marks with `isTrusted: false`. A `MotionEvent` pair, down and up 60 ms apart, dispatched with `WebView.DispatchTouchEvent` at the centre of the button arrives as a trusted touch and is accepted. The page reports the rectangle in CSS pixels, and the app multiplies it by `devicePixelRatio` (2.75 on this phone).
- The Skip button was `button.ytp-ad-skip-button-modern`, about 67 by 32 CSS pixels, at the same position in all 21 cases over two days. It appeared 5.6 to 7.4 s after the break started when it was on the first ad of the break.
- Every saved Skip snapshot showed the ad's position in its break: "1 of 2" 18 times, "2 of 2" 3 times. The player keeps `ad-showing` across the ads of a break, so a break is reported as one `ad.started` and one `ad.ended`. When the button was on the second ad, the first one played without a button for 6.6 to 21 s. After a skip on "1 of 2", no ad started again until the next break.
- `ad.started` could never tell whether a break would be skippable. The Skip button is the only signal.
- The first journal version reported a skip whenever one was requested, even when the page ignored the click. Whether a skip worked must be decided from what happens next: the ad ending within 1 s.

### Survival in the background (step 0.7)

The app ran from 17 October 02:13 to 18 October 14:39 with "Don't keep activities" off and no battery optimisation exemption. The spike started the Media3 session only from a button, which was pressed in 2 of the 9 processes, so most background time ran without a foreground service.

| Started | Session service | Background | End | Reason reported by Android |
| --- | --- | --- | --- | --- |
| 17 Oct 02:13 | No | 27 min | 02:44 | HyperOS `LockScreenClean`, 10 min after the screen turned off |
| 13:34 | Yes | 30 min | 14:05 | `LockScreenClean`, 10 min after the screen turned off |
| 16:34, by the system for the service only | Destroyed at 16:37 | Not applicable | 16:45 | `LockScreenClean`, 10.6 min after the screen turned off |
| 18:08 | No | 20 min | 18:29 | `LockScreenClean`, 10 min after the screen turned off |
| 20:54 | No | 8 min | 21:02 | Low memory: the WebView renderer process, then the app process |
| 21:14 | Yes, in the foreground from 21:14:51 to 22:21:31 | 75 min | 22:37 | Excessive CPU while cached, 15.6 min after the foreground service stopped |
| 22:59, Activity restored from saved state | No | 48 min | 23:57 | Excessive CPU while cached, during playback |
| 18 Oct 10:32, Activity restored from saved state | No | 38 min | 11:38 | `LockScreenClean`, 10 min after the screen turned off |
| 12:05 | No | 35 min | 12:41 | `LockScreenClean`, 10 min after the screen turned off |

- Android never destroyed the Activity or the WebView while the process lived, in about five hours of background time. The only `activity.destroyed` events came from `LockScreenClean` in the same second as the process kill.
- Every kill happened while the process was cached (importance 400): no foreground service was running at any kill.
- HyperOS's `ProcessSceneCleaner` logs `LockScreenClean: kill-background` and kills cached processes 10 minutes after the screen locks. The one screen-off period longer than 10 minutes without a kill was in a process with the session service, which suggests that the foreground service protects the process; it is not proven yet.
- Android's excessive CPU check killed cached processes twice, with a limit of 5 % of one core over 5 minutes. The check counts from the moment the process leaves the foreground state, and it never fired while the foreground service ran.
- The page keeps running at full rate in the background, also after playback stops, because Chromium believes the WebView is visible. The app process and its WebView renderer held 300 to 500 MB together.
- Once, the system started the process for the session service alone, without an Activity: the service is exported for media controllers. A session service without a page cannot play anything.
- After a process death, Android recreated the Activity from its saved state when the app was reopened from the recent apps, but the video and its position were lost.

## Implementation notes for later phases

- .NET for Android names Java wrappers after a hash of the namespace. Components that the system or other apps address by name, such as the Media3 service, need a stable name through the `Name` property of `[Service]` or `[Activity]`.
- `Xamarin.AndroidX.Media3.Session` places the `Player` constants in `BasePlayer.InterfaceConsts`, and every builder result is annotated as nullable.
- Guava's `Futures` is not exposed by the .NET binding. `CallbackToFutureAdapter` from AndroidX returns a completed `ListenableFuture` to Media3.
- The WebKit binding exposes Java's `ScriptHandler` as `IScriptHandler`. `DOCUMENT_START_SCRIPT` and `WEB_MESSAGE_LISTENER` are both supported on WebView 155.
- Page `console` messages reach `logcat` under the tag `chromium`.
- The Debug build exposes the WebView remote debugging socket, which is a useful test tool; it must stay disabled in Release builds.
- HyperOS refuses USB installs until **Developer options → Install via USB** is enabled. On a phone with more than one Android user, a restriction on a secondary user can make `adb install` fail with `INSTALL_FAILED_USER_RESTRICTED`; installing for the owner only (`AndroidDeviceUserId=0`) works.

## Decisions

| Decision | Record |
| --- | --- |
| Keep playback alive with a native window visibility override in `KeepVisibleWebView`, drop the visibility shield script, and switch the override off when background playback is released | [ADR 0012](../adr/0012-native-window-visibility-for-background-playback.md) |
| Press Skip with a native tap at the rectangle the page reports. The page decides nothing; the coordinator decides whether a skip worked | [ADR 0013](../adr/0013-native-tap-to-press-skip.md) |
| Pin Media3 1.11.1, WebKit 1.14.0.1 and the AndroidX packages they need, and require Android 6.0 (API 23) | [ADR 0014](../adr/0014-androidx-alignment-and-android-6-minimum.md) |
| Keep the default WebView user agent; the variants without `; wv` are the first fallback if Google blocks sign-in | [ADR 0002](../adr/0002-host-youtube-mobile-site-in-webview.md), status updated |
| Media3 session service with a `SimpleBasePlayer` facade, as planned | [ADR 0006](../adr/0006-media3-session-service-for-background-playback.md), status updated |
| The foreground service starts by itself when the page reports `playing`, never only from a button | [ARCHITECTURE.md, section 11](../architecture/ARCHITECTURE.md#background-playback), risk R-06 |
| The notification seek bar is not built | Owner decision; not planned |

## Open points

| Open point | Covered by |
| --- | --- |
| A long screen-off run with the foreground service, including whether HyperOS `LockScreenClean` spares a process with a running media foreground service | Field test, roadmap step 4.7 |
| A native tap with the screen off | Step 4.7 |
| The CPU and battery cost of the always-visible WebView | Steps 4.6 and 4.7, risk R-16 |
| The Android 17 restriction on background audio, which Android 14 cannot show | Phase 4, on an Android 17 emulator or device |
| A media controller that starts the session service while no page exists | Step 4.4 |
| Reopening the last video after a process death | Step 6.5 |
| Sign-in survival across app restarts was not recorded; nine process starts in everyday use brought no reported sign-out. Sign-in without third-party cookies was not tested | Step 3.8 |
| The WebView remote debugging socket disabled in Release builds | Step 3.1 |
| Mediator 3.0.2 with trimming, planned outside the spike | Step 1.6 |
