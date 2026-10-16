# ADR 0009: Distribution through GitHub Releases and Obtainium

- **Status:** Accepted
- **Date:** 2026-10-16

## Context

The app is for the owner and possibly a few friends. App store publishing is not wanted. Updates must still reach every phone without manual downloads.

## Decision

- Every merge into `master` builds a signed APK in GitHub Actions and publishes it as a GitHub Release tagged `vMAJOR.MINOR.PATCH`.
- Phones install and update the app with Obtainium, which watches the repository's releases and notifies when a new version is available.
- The repository is public: GitHub-hosted runners are free for public repositories, and friends can see the releases.

## Consequences

- No store review, no store fees.
- The signing key must never change. Losing it means every user must uninstall before installing a new version. The keystore is backed up offline.
- Users must allow installing apps from Obtainium once.
