# ADR 0004: No backend

- **Status:** Accepted
- **Date:** 2026-10-16

## Context

A backend could proxy YouTube requests, sync settings across devices, or host selector updates.

- Proxying YouTube from a server is counterproductive: YouTube rate-limits and blocks automated traffic, especially from data-centre IP ranges, and the user's own signed-in session lives on the phone.
- The app has one user and one device. There is nothing to sync.
- Selector updates can be served as a static JSON file from this public GitHub repository.

## Decision

The app runs entirely on the device. There is no server component.

## Consequences

- No hosting cost, no server to secure or maintain.
- Selector updates are fetched from a raw file URL in the repository; they are data, never executable code.
- If cross-device sync becomes a real need, a backend can be introduced in a new ADR.
