# ADR 0014: Pin AndroidX versions for Media3 and WebKit, and require Android 6.0

- **Status:** Accepted
- **Date:** 2026-10-18

## Context

The [feasibility spike](../spikes/0001-feasibility.md) added Media3 and AndroidX WebKit to a MAUI 10 app and found two version conflicts:

- Every `Xamarin.AndroidX.Media3.Session` release from 1.9 on needs AndroidX Lifecycle 2.10 or later, while packages pulled in by MAUI 10 pin the 2.9 series. NuGet reports `NU1608` and resolves a mixed set of versions.
- `Xamarin.AndroidX.WebKit` 1.15 and later pull AndroidX Lifecycle and Collection versions outside the constraints of MAUI 10. Version 1.14.0.1 does not, and it supports document-start scripts and origin-restricted message listeners on WebView 155.

Nine explicit AndroidX versions align the set: `Collection.Ktx` 1.6.0.1, `SavedState.SavedState.Ktx` 1.5.0.1, and the Lifecycle packages `LiveData`, `LiveData.Core`, `LiveData.Core.Ktx`, `Process`, `Runtime.Ktx`, `ViewModel.Ktx` and `ViewModelSavedState` at 2.11.0.1. AndroidX Lifecycle 2.11 requires API 23, so the minimum Android version rises from 5.0 (API 21) to 6.0 (API 23).

## Decision

- Use `Xamarin.AndroidX.Media3.Session` 1.11.1 and `Xamarin.AndroidX.WebKit` 1.14.0.1.
- Pin the nine AndroidX packages above in `Directory.Packages.props`, in a labelled item group with a comment that names the reason. Central transitive pinning, already enabled, applies these versions to transitive references, so projects do not reference them directly.
- Set `SupportedOSPlatformVersion` to 23 in the Android projects when they are created in roadmap step 1.2.
- The pins are added when the packages are first referenced, in roadmap steps 3.2 and 4.2.

## Consequences

- Builds resolve one consistent AndroidX graph without `NU1608`, and every forced version is visible and reviewed in one file.
- Phones older than Android 6.0 cannot install the app.
- The pins can hold packages back. Every MAUI, Media3 or WebKit update re-checks the group, and the .NET 11 migration in phase 8 revisits it. Dependabot proposes these updates together with the rest of the NuGet group.
