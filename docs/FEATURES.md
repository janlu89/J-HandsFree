# Features

Priorities use MoSCoW: **Must**, **Should**, **Could**, **Won't** (for this version). Effort is a rough size: S (a day or less), M (a few days), L (a week or more).

## Requested features

| ID | Feature | Priority | Effort | Phase | Acceptance criteria |
| --- | --- | --- | --- | --- | --- |
| F-01 | Background playback | Must | L | 4 | Audio continues for one hour with the screen off, and while another app is in the foreground. |
| F-02 | Automatic skip of skippable ads | Must | M | 5 | Every skippable ad is skipped within one second of the Skip button appearing. Non-skippable ads play normally. |
| F-03 | Signed-in YouTube session | Must | M | 3 | Subscriptions and personal playlists are available. Fallback if sign-in is blocked: unlisted playlists opened by link. |
| F-04 | Continuous playlist playback | Must | S | 3 | A playlist advances to the next item without interaction. |
| F-05 | System media controls | Must | M | 4 | Play, pause, next and previous work from the lock screen, the notification, headphones and car Bluetooth. |
| F-06 | Auto-dismiss idle prompt | Must | S | 4 | The "Continue watching?" prompt never stops a session. |
| F-07 | Navigation prompts duck the audio | Must | S | 4 | During navigation voice prompts the audio gets quieter and then returns. Verified in the field test; Android performs ducking automatically for apps that request audio focus correctly. |

## Candidate features

These were researched for a hands-free, in-car listening use case. The phase 6 selection is confirmed before the phase starts.

| ID | Feature | Priority | Effort | Phase | Why it helps | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| C-01 | Drive mode screen | Should | S | 6 | Large play, pause, next and seek controls that can be hit without looking. | High contrast, dark, few controls. |
| C-02 | Sleep timer | Should | S | 6 | Stop after N minutes or at the end of the current video. | Common in podcast apps. |
| C-03 | Home screen shortcuts | Should | S | 6 | One tap from the home screen starts a favourite playlist. | Android app shortcuts with the playlist URL. |
| C-04 | Seek buttons in notification | Should | S | 6 | Back 15 seconds and forward 30 seconds without unlocking. | Media3 custom commands. |
| C-05 | Configurable headphone presses | Should | M | 6 | Double press chooses next video or forward 30 seconds; triple press chooses previous or back 15 seconds. | Media3 media button handling. |
| C-06 | Resume last session | Should | M | 6 | On launch, offer to continue the last video at the saved position. | YouTube links accept a start time parameter. |
| C-07 | Diagnostics and log export | Should | S | 5 | When YouTube changes, the log shows what broke. | Shared through the Android share sheet. |
| C-08 | Selector health warning | Should | S | 5 | Notifies when an ad looks skippable but no Skip button is found, a sign that markup changed. | Uses events already in the contract. |
| C-09 | Remote selector updates | Should | M | 6 | Fix a broken selector without releasing a new APK. | Versioned JSON in this repository. Data only, never code. |
| C-10 | Time saved statistics | Could | S | 5 | Shows how much ad time was skipped. | Built on `SkipRecord`. |
| C-11 | Sponsor segment skipping | Could | M | 6 | Skips sponsor reads, self-promotion, intros and outros inside videos, using crowd-sourced segments. | SponsorBlock public API. Its hash-prefix endpoint hides which video is being watched. Review the data license and add attribution before use. |
| C-12 | Playback speed per channel | Could | S | 6 | Podcasts at 1.25x, music at 1x, remembered per channel. | `player.setRate` command already exists. |
| C-13 | Picture-in-picture | Could | M | 6 | A small video window over the navigator when wanted. | Android PiP mode for the activity. |
| C-14 | Data saver in background | Could | M | Research | Lowest video quality while the screen is off. | Needs research: quality control in the mobile website may not be scriptable reliably. |

## Won't have in this version

| ID | Feature | Reason |
| --- | --- | --- |
| W-01 | Android Auto | The owner's car does not support it. It would need a media library service with a browse tree. |
| W-02 | Voice assistant commands | Needs a media browse service; revisit after W-01. |
| W-03 | Auto-resume when the car connects | Android restricts starting a foreground service from the background. Revisit after phase 4 with the platform's exemptions. |
| W-04 | Offline downloads | Requires stream extraction, which contradicts [ADR 0002](adr/0002-host-youtube-mobile-site-in-webview.md). |
| W-05 | Ad blocking | Explicit non-goal. The app only presses Skip when YouTube offers it. |
| W-06 | Cross-device sync | No backend, see [ADR 0004](adr/0004-no-backend.md). |
| W-07 | iOS | Android only. |

## Sources

- [SponsorBlock](https://sponsor.ajay.app) and its [API documentation](https://wiki.sponsor.ajay.app/w/API_Docs)
- [Android media controls](https://developer.android.com/media/implement/surfaces/mobile)
- [Media3 background playback](https://developer.android.com/media/media3/session/background-playback)
- [Android 17 background audio hardening](https://developer.android.com/about/versions/17/changes/bg-audio)
