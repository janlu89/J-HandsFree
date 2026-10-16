# ADR 0006: Media3 session service for background playback

- **Status:** Accepted, to be validated by the phase 0 spike
- **Date:** 2026-10-16

## Context

Android stops or freezes apps that play audio without a visible activity unless they run a foreground service. Starting with Android 17, audio from an app without a foreground service of the right kind is restricted, and the service must be started while the app is visible.

The app also needs system media controls: the lock screen, the media notification, headphone buttons and car Bluetooth controls.

The media is played by the WebView, not by a native player, so the system has no `Player` object to attach controls to.

## Decision

- Use AndroidX **Media3** through the .NET bindings.
- Implement `PlaybackService` as a Media3 `MediaSessionService`. Media3 manages the foreground service of type `mediaPlayback` and the media notification.
- Implement `WebViewSessionPlayer` as a Media3 `SimpleBasePlayer`: a player facade whose state is published by `PlaybackCoordinator` and whose commands (play, pause, next) are forwarded back to the coordinator.
- Start playback only from the visible UI, so the service starts with while-in-use capability.
- Declare `FOREGROUND_SERVICE` and `FOREGROUND_SERVICE_MEDIA_PLAYBACK` in the manifest.

## Consequences

- Notification, lock screen and Bluetooth controls come from the platform library instead of hand-written code.
- The facade must report accurate state; the state machine in the Domain is the single source of truth.
- If subclassing `SimpleBasePlayer` from .NET proves impractical in the spike, the fallback is a hand-written foreground service with a `MediaSessionCompat`. That would be recorded in a new ADR.
