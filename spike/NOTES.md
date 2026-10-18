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

**Result: confirmed with a native tap; a script click is ignored.** Risk R-05 occurred and its planned fallback resolves it. Over two days of everyday use, 20 native taps out of 20 closed the ad, with a median of 211 ms from the Skip button appearing to the ad ending.

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

First session, 17 October:

| Time | Mode | Skip button shown after | Result |
| --- | --- | --- | --- |
| 02:00:37 | Click | 5.9 s | The app requested the skip after 3 ms and the script clicked after 12 ms, but the button stayed for another 7.3 s: the click was ignored. The ad lasted 13.1 s. |
| 02:04:18 | Tap | 5.8 s | Skip target received after 52 ms, tap at (944, 643) physical pixels, button gone 165 ms after it appeared, ad ended. |

Everyday use, from 17 October 13:35 to 18 October 12:23, native tap only:

| Measure | Result |
| --- | --- |
| Ad breaks | 23 |
| Breaks with a Skip button | 19, each pressed with a native tap |
| Confirmed skips (`ad.ended` within 1 s of the request) | 19 of 19 |
| Breaks without a Skip button | 3, played to the end in 23 to 33 s; one more break has no recorded end |
| Skip button shown after the break started | Median 5.8 s; 5.6 to 7.4 s when the button was on the first ad of the break, 12.7 to 26.9 s when it was on the second |
| Skip button appearing to button gone, page clock | Median 211 ms, maximum 339 ms |
| Skip request to ad end, app clock | Median 187 ms, maximum 324 ms |
| Taps with the app in the background (another app in front, screen on) | 17 of 19 |
| Taps with the screen off | None: no break started while the screen was off |

Together with the first session: 20 native taps, 20 confirmed skips, no miss.

Where the time goes, as medians of the 19 taps: Skip button reported to skip request 3 ms, request to button rectangle received 27 ms, rectangle to tap 9 ms, tap to button gone 167 ms.

### Ad breaks with two ads

Every saved Skip snapshot (21, including the first session) shows the position of the ad in its break next to the ad badge: "1 of 2" 18 times and "2 of 2" 3 times.

- The player keeps the class `ad-showing` across the ads of a break, so the watcher reports a whole break as one `ad.started` and one `ad.ended`.
- When the Skip button was on the second ad, the first ad played without a button for 6.6 to 21 s.
- After a skip on "1 of 2", and after the ignored click on "1 of 2" in the first session, the player left ad mode and no ad started again until the next break, at least 3 minutes later. The data does not show whether YouTube dropped the second ad or never scheduled it.

### Findings

- YouTube ignores clicks raised by scripts (`isTrusted` is `false`). A touch dispatched to the WebView arrives as a trusted event and is accepted.
- The native tap path is fast and reliable: 20 of 20 confirmed skips, at most 354 ms from the Skip button appearing to the ad ending, well inside the one-second target of F-02.
- The tap works while the app is in the background: 17 of the 19 everyday taps were dispatched while the Activity was stopped and another app was in front, and all of them closed the ad.
- `ad.started` always reported `kind: unknown`. The page cannot tell at the start whether a break will be skippable; the Skip button is the only signal.
- The Skip button kept the same classes (`button.ytp-ad-skip-button-modern`), size (67 by 32 CSS pixels) and position in all 21 cases over two days.
- The `ad.skip` mode `locate` and the `ad.skipTarget` event already in the message contract are exactly what the fallback needs.
- The first journal version reported `skipped: true` whenever a skip was requested, even when the page ignored it. `ad.ended` now reports a skip only when the ad ends within 1 s of the request, and adds `msFromSkipRequestToEnd` and `offeredSkip`.
- Open: a tap with the screen off was never exercised, because no break started while the screen was off. It moves to the car field test (roadmap step 4.7), together with the long screen-off run deferred in step 0.4.

## Step 0.7: one hour in the background

**Result: the Activity and the WebView are never destroyed on their own; a process left in the background without the foreground service is killed whole, within 8 to 75 minutes.** Assumption 5 of the architecture holds, but the fallback it names, moving the WebView to a longer-lived component, would not help: every loss took the whole process, service included.

### Setup

- Spike build of commit `8a53c01`, in everyday use from 17 October 02:13 to 18 October 14:39, mostly with another app in front, including a car journey with a navigator.
- `spike-journal.jsonl` records each process start with the exit reasons of the earlier processes (`ApplicationExitInfo`), Activity, WebView and service lifecycle, screen on and off, and a heartbeat every 5 minutes. A heartbeat gap means the process was frozen or the CPU was suspended.
- Phone settings: "Don't keep activities" off, no battery optimisation exemption, background running allowed (app op `RUN_ANY_IN_BACKGROUND` at its default, allow). The standby bucket was 10 (active) at the start and 40 (rare) when read after the run.
- The spike starts the Media3 session only from its Session button, which was pressed in 2 of the 9 processes. In the other 7, playback ran in the background without a foreground service.
- The device logs (`logcat` events buffer and `dumpsys activity exit-info`) still covered the kills from 17 October 21:14 on.

### Processes

Two restarts at install time are left out. "Background" is the time from the last `activity.stopped` to the end of the process.

| Started | Started by | Session service | Background | End | Reason reported by Android |
| --- | --- | --- | --- | --- | --- |
| 17 Oct 02:13 | User | No | 27 min | 02:44 | HyperOS `LockScreenClean`, 10 min after the screen turned off |
| 13:34 | User | Yes | 30 min | 14:05 | `LockScreenClean`, 10 min after the screen turned off |
| 16:34 | The system, for the session service only; no Activity | Service destroyed at 16:37 | Not applicable | 16:45 | `LockScreenClean`, 10.6 min after the screen turned off |
| 18:08 | User | No | 20 min | 18:29 | `LockScreenClean`, 10 min after the screen turned off |
| 20:54 | User | No | 8 min | 21:02 | The WebView renderer process was killed for low memory, and the app process ended with signal 9 0.4 s later |
| 21:14 | User | Yes, in the foreground from 21:14:51 to 22:21:31 | 75 min | 22:37 | Excessive CPU: 15.9 s in 5 min against a limit of 5 %, 15.6 min after the foreground service stopped |
| 22:59 | User, Activity restored from saved state | No | 48 min | 23:57 | Excessive CPU: 27.8 s in 5 min; the WebView renderer process 67.9 s, also killed |
| 18 Oct 10:32 | User, Activity restored from saved state | No | 38 min | 11:38 | `LockScreenClean`, 10 min after the screen turned off |
| 12:05 | User | No | 35 min | 12:41 | `LockScreenClean`, 10 min after the screen turned off |

Every kill happened with importance 400 (cached): no foreground service was running at any kill. With a page loaded, the app process used 214 to 313 MB (PSS) and the WebView renderer process 95 to 184 MB.

### Findings

- Android never destroyed the Activity or the WebView while the process lived, in about five hours of background time across eight processes, the longest 75 minutes. There was no `webview.disconnected` at all. The two `activity.destroyed` events (`finishing: true`) came from `LockScreenClean` in the same second as the process kill.
- HyperOS's `ProcessSceneCleaner` logs `LockScreenClean: kill-background` and kills cached processes 10 minutes after the screen locks. This is risk R-06. The one screen-off period longer than 10 minutes without a kill (17 October 13:39 to 13:55) was in a process that had the session service, which suggests that the foreground service protects the process; the device logs for that time had already rotated, so it is not proven.
- Android's excessive CPU check killed the cached process twice, with a limit of 5 % of one core over 5 minutes. The first kill came 15.6 minutes after Media3 left the foreground state, which it does 10 minutes after playback stops (step 0.5): the foreground service itself ran for 67 minutes without a problem. The second kill came while playback ran without any session service; the last ad was skipped 2.5 minutes before the kill, and the owner paused playback shortly before it.
- The CPU check applies only to background processes: the kill messages count the time from the moment the process left the foreground state (`dur=935318`, exactly the 15.6 minutes since the foreground service stopped), and the process was never killed while the foreground service ran.
- The page keeps using CPU in the background, also after playback stops: Chromium believes the WebView is visible (step 0.3), so it keeps the page running at full rate. The battery cost must be measured in phase 4.
- Heartbeats were regular whenever the screen was on. With the screen off and no playback they often stopped or came late (gaps of 8.9 to 13.7 minutes), which means the process was frozen or the CPU suspended.
- Once the system started the process for the session service alone, without an Activity, and destroyed the service 2.5 minutes later. The service is exported for media controllers, so another app or the system connected to it; the journal does not show which. A session service without a page cannot play anything.
- After the two CPU kills, the app was reopened from the recent apps and Android recreated the Activity from its saved state. The spike then loads the start page again, so the video and its position were lost.
- Low memory ended one process: the system killed the WebView renderer process, then the app process. The app and its renderer together hold 300 to 500 MB.

## Implications for later steps

- The native tap is the primary skip method, not a fallback: `AdSkipCoordinator` should send `ad.skip` with mode `locate`, and the bridge adapter performs the tap. Script clicks can be dropped. ARCHITECTURE section 9 and the phase 0 report must say so.
- The notification seek bar needs the facade to publish duration and position and to handle seeking. The message contract offers only the relative `player.seekBy`; an absolute `player.seekTo` command is missing and must be added in step 0.8 or phase 3.
- The native window visibility override, not the page script, is what keeps playback alive. Section 9 of the architecture and ADR 0007 describe the visibility shield as a page script; step 0.8 must record the change in a new ADR, and `KeepVisibleWebView` becomes part of Infrastructure.Android.
- Sign-in works with the default user agent, so the real app keeps it unchanged. Changing the user agent is a fallback only, because it can also change the markup YouTube serves and break the selectors.
- The phone runs Android 14, not 17. The Android 17 background audio restrictions described in ADR 0006 cannot be observed on it; confirming them needs an Android 17 emulator or device. Starting playback from the visible UI stays the rule, because Android 14 already requires a typed foreground service.
- Risk R-06 occurred: HyperOS kills cached apps 10 minutes after the screen locks. The foreground service is therefore mandatory whenever audio plays, and it must start by itself when the page reports `playing`, as the background playback flow in ARCHITECTURE section 11 already describes, never only from a button. The one-time prompt to exclude the app from battery optimisation (roadmap step 4.3) stays.
- Assumption 5 needs no fallback: the Activity survives, and moving the WebView to a longer-lived component would not survive a process kill. What the real app needs instead is to reopen the last video after a process death (feature "resume the last session", roadmap step 6.5).
- When the pause timeout releases background playback, the WebView should stop reporting itself as visible, so Chromium throttles the paused page. Otherwise the cached process keeps using CPU and Android kills it about 15 minutes later.
- A media controller can start the session service while no page exists. Phase 4 must decide what the service does then, for example refuse the command or open the app.
- `ad.started` cannot tell whether a break is skippable, and a break of two ads is one break. `AdBreak` starts as `AdKind.Unknown`, becomes skippable when the Skip button appears, and covers the whole break.
- The car field test (roadmap step 4.7) must cover what the spike could not: the screen off for more than 10 minutes with the foreground service running, a native tap with the screen off, and the battery cost of the always-visible WebView.
