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

### Implications for later steps

- The phone runs Android 14, not 17. The Android 17 background audio restrictions described in ADR 0006 cannot be observed on it; confirming them needs an Android 17 emulator or device. Starting playback from the visible UI stays the rule, because Android 14 already requires a typed foreground service.
- Xiaomi's battery manager is known to stop background apps (risk R-06). Steps 0.4 and 0.7 must record the HyperOS battery and autostart settings used for each test.
