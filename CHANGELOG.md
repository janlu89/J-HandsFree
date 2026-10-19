# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Domain value objects: `VideoId`, which accepts only the eleven URL-safe characters of a YouTube video id, `VideoMetadata` with an optional duration for live streams, and `AdBreak`, whose kind stays unknown until the Skip button appears or the break ends.
- `PlaybackSession`, the playback state machine from the architecture. A request that the current state does not allow returns an invalid `TransitionResult` and changes nothing.
- `PlayerCommand` records for the YouTube page and the `IPlayerCommands` port, with a validated `SkipTarget` for the native Skip tap.
- `SkipPolicy`, which allows a skip only for a skippable break while auto skip is enabled, with a delay of up to ten seconds, and `PauseTimeoutPolicy`, which releases background playback after a pause of ten minutes by default.

### Changed

- Dependabot no longer proposes TypeScript 6.1 or later, which typescript-eslint does not accept yet.
- Roadmap, overview page, risk register and README record the end of phase 1 and release v0.1.0. The development setup explains why a local build cannot update a release installed from GitHub.

## [0.1.0] - 2026-10-18

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
- One dependency injection extension method per layer, wired in `MauiProgram` together with the source-generated Mediator 3.0.2. The container validates every registration at startup.

### Changed

- Both branch rulesets require their status checks, restricted to GitHub Actions. A pull request into `master` from any branch other than `dev` is blocked.
- Architecture, message contract, risk register, roadmap and overview page updated with the spike results. ADRs 0002, 0006 and 0007 point to the results.
- The CI and release workflows fail when the solution, the player scripts project or the app project is missing, instead of skipping with a notice.
- The signing instructions describe the PKCS12 keystore, which has one password for the store and the key, and the `release` environment, which GitHub creates without a branch restriction.
