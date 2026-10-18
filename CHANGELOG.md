# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Project documentation: architecture, decision records, roadmap, features, risks, coding standards, testing strategy, git workflow and CI/CD.
- Repository configuration: MIT license, editor configuration, central package management and build properties.
- GitHub Actions workflows for continuous integration, source branch guarding and releases.
- Phase 0 feasibility report, `docs/spikes/0001-feasibility.md`, with the results measured on a real phone.
- ADR 0012: background playback through a native window visibility override instead of a page script.
- ADR 0013: Skip pressed with a native tap at the position the page reports, instead of a script click.
- ADR 0014: AndroidX versions pinned for Media3 and WebKit, and Android 6.0 as the minimum version.
- Solution `JHandsFree.slnx` with the five source projects: Domain, UseCases, Infrastructure.PlayerProtocol, Infrastructure.Android and the MAUI app Mobile, which runs on Android 6.0 and later and shows its name and version.
- Test projects for Domain, UseCases and Infrastructure.PlayerProtocol with xUnit v3, Shouldly and NSubstitute. Each starts with a test that fails when the assembly under test references something the dependency rule forbids.
- Player scripts project, `src/player-scripts`, with TypeScript, esbuild, Vitest with jsdom, ESLint and Prettier. ESLint rejects `any`, non-null assertions, `eval` and network calls.
- The build of Infrastructure.Android installs and bundles the player scripts with npm when they change, and embeds the bundle as the resource `JHandsFree.Infrastructure.Android.player-scripts.js`.

### Changed

- Both branch rulesets require their status checks, restricted to GitHub Actions. A pull request into `master` from any branch other than `dev` is blocked.
- Architecture, message contract, risk register, roadmap and overview page updated with the spike results. ADRs 0002, 0006 and 0007 point to the results.
