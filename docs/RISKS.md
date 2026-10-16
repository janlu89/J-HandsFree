# Risk register

Likelihood and impact are rated **High**, **Medium** or **Low**. The register is reviewed at the end of every phase.

| ID | Risk | Likelihood | Impact | Mitigation | Checked in |
| --- | --- | --- | --- | --- | --- |
| R-01 | Google blocks sign-in inside an embedded WebView ("This browser or app may not be secure"). | High | High | Test the default user agent and one without the `; wv` marker. Fallback: personal playlists set to unlisted, opened by link without signing in. | Phase 0 |
| R-02 | Android freezes or mutes audio from an app without a valid foreground service (enforced from Android 17). | Medium | High | Media3 `MediaSessionService` with type `mediaPlayback`, always started from the visible UI. Keep the service during short pauses, stop it after the pause timeout. | Phase 0, 4 |
| R-03 | The Activity is destroyed in the background and takes the WebView with it. | Medium | High | Measure in the spike. Fallback: the WebView is owned by a longer-lived component and re-attached to the page when visible. | Phase 0 |
| R-04 | YouTube changes its page markup and selectors stop matching. | High | Medium | All selectors in one module; snapshot tests; health warning; remote selector updates as data. | Phase 3, 5, 6 |
| R-05 | The Skip button ignores script clicks because they are not trusted events. | Medium | High | Fallback to a native tap: the script reports the button rectangle and the bridge dispatches a `MotionEvent`. | Phase 0 |
| R-06 | The phone maker's battery manager kills the app despite the foreground service. | Medium | Medium | One-time prompt to exclude the app from battery optimization; link to vendor-specific guidance in the README. | Phase 4 |
| R-07 | Android System WebView on the device lacks the WebKit features the app needs. | Low | High | Feature detection at startup with a clear message to update Android System WebView. | Phase 3 |
| R-08 | .NET bindings for Media3 or AndroidX WebKit lag behind or miss APIs. | Medium | Medium | Verify in the spike. Fallback: a small Java binding or `MediaSessionCompat`. | Phase 0 |
| R-09 | The signing key is lost. | Low | High | Offline backup of the keystore and passwords in a password manager. | Phase 1 |
| R-10 | YouTube treats the automation as a Terms of Service violation and limits the account. | Low | Medium | Personal use only; no store publishing; human-like timing (skip only after the button appears). Accepted risk. | Ongoing |
| R-11 | YouTube changes ad formats, for example fewer skippable ads or no Skip button in server-inserted ads. | Medium | Medium | Out of the app's control. The app keeps working as a background player even if no ad is skippable. | Ongoing |
| R-12 | The .NET 11 migration introduces runtime regressions on Android (CoreCLR becomes the default). | Medium | Medium | Dedicated phase with a field test; .NET 11 can temporarily fall back to Mono with an MSBuild property while an issue is reported. | Phase 8 |
| R-13 | A secret is committed to the public repository. | Low | High | Secrets only in GitHub Actions; `.gitignore` covers keystores; enable GitHub secret scanning and push protection. | Phase 1 |
| R-14 | Scope creep for a solo developer. | Medium | Medium | MoSCoW priorities in [FEATURES.md](FEATURES.md); phase 6 selection confirmed before starting. | Every phase |
| R-15 | Mediator 3.0.2 predates .NET 10; explicit .NET 10 support is listed only in the 3.1 release candidate. | Low | Low | The package targets .NET Standard 2.0. Phase 1 verifies generation and a trimmed Release build; if it fails, move to the latest stable 3.x. | Phase 1 |
