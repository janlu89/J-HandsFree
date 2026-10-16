# Security Policy

## Supported versions

Only the latest release published on GitHub receives fixes.

## Reporting a vulnerability

Please do not open a public issue for security problems. Use GitHub's **Report a vulnerability** button in the **Security** tab of this repository to send a private report. You will receive an answer within seven days.

## How this project handles secrets

- The Android signing keystore and its passwords are stored only as GitHub Actions secrets and in an offline backup. They are never committed.
- Release builds run only on pushes to `master`, so secrets are never exposed to pull requests from forks.
- The app stores no credentials of its own. Signing in to YouTube happens on the official website inside the WebView, and the session cookies stay in the WebView's private storage on the device.
- Injected scripts exchange messages with the app only from the allowed YouTube origin.
