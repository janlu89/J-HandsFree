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
- With only the shield, the page never learned it was hidden, yet the video paused within milliseconds and resumed by itself on return. The cause is not determined. The only control run without shield and native override lasted 3 s without a pause, so it neither confirms nor rules out a pause triggered by Chromium; a control run of at least 30 s is needed to settle it.
- Opening the YouTube app paused playback: another media app takes the audio focus, which is expected Android behaviour. Waze in front did not pause it.
- Background runs lasted at most 12 s. The process moved to process states 15 and 16 (last and cached activity) and was never frozen in that time. Whether Android stops playback after minutes without a foreground service was not tested; step 0.4 covers it.
- The process restarted once during the test (new pid at 01:02:46); the cause was not recorded.
- Implementation notes: the binding exposes Java's `ScriptHandler` as `IScriptHandler`; `Xamarin.AndroidX.WebKit` 1.15 and later pull AndroidX Lifecycle and Collection versions outside the constraints of MAUI 10 (NU1608), while 1.14.0.1 does not; page `console` messages reach logcat under the tag `chromium`.

## Implications for later steps

- The native window visibility override, not the page script, is what keeps playback alive. Section 9 of the architecture and ADR 0007 describe the visibility shield as a page script; step 0.8 must record the change in a new ADR, and `KeepVisibleWebView` becomes part of Infrastructure.Android.
- Sign-in works with the default user agent, so the real app keeps it unchanged. Changing the user agent is a fallback only, because it can also change the markup YouTube serves and break the selectors.
- The phone runs Android 14, not 17. The Android 17 background audio restrictions described in ADR 0006 cannot be observed on it; confirming them needs an Android 17 emulator or device. Starting playback from the visible UI stays the rule, because Android 14 already requires a typed foreground service.
- Xiaomi's battery manager is known to stop background apps (risk R-06). Steps 0.4 and 0.7 must record the HyperOS battery and autostart settings used for each test.
