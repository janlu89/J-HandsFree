# J-HandsFree documentation

Start with the architecture, then read the roadmap for the order of work.

## Product

| Document | Purpose |
| --- | --- |
| [FEATURES.md](FEATURES.md) | Requested features, researched candidates and their priority |
| [ROADMAP.md](ROADMAP.md) | Phases, steps inside each phase and definition of done |
| [RISKS.md](RISKS.md) | Risk register with impact, likelihood and mitigation |

## Architecture

| Document | Purpose |
| --- | --- |
| [architecture/ARCHITECTURE.md](architecture/ARCHITECTURE.md) | Layers, components, flows, state machine, message contract |
| [architecture/overview.html](architecture/overview.html) | The same plan as a standalone visual page; open it in a browser |
| [adr/](adr/) | Architecture decision records |
| [spikes/0001-feasibility.md](spikes/0001-feasibility.md) | Phase 0 feasibility spike: results on a real phone and the decisions they forced |

## Engineering

| Document | Purpose |
| --- | --- |
| [CODING_STANDARDS.md](CODING_STANDARDS.md) | Clean Code and Clean Architecture rules for C# and TypeScript |
| [TESTING.md](TESTING.md) | Test pyramid, naming, tools and what must be covered |
| [GIT_WORKFLOW.md](GIT_WORKFLOW.md) | Branches, protections, merge strategy, commits, versioning |
| [CI_CD.md](CI_CD.md) | Workflows, required checks, secrets, signing, releases |
| [DEVELOPMENT_SETUP.md](DEVELOPMENT_SETUP.md) | Tools to install and how to run the app on a phone |

## Decision log

| ADR | Decision |
| --- | --- |
| [0001](adr/0001-record-architecture-decisions.md) | Record architecture decisions |
| [0002](adr/0002-host-youtube-mobile-site-in-webview.md) | Host the YouTube mobile website in a WebView (visibility shield replaced by 0012) |
| [0003](adr/0003-clean-architecture-without-cqrs.md) | Clean Architecture with ports and adapters, without CQRS (mediator part superseded by 0011) |
| [0004](adr/0004-no-backend.md) | No backend |
| [0005](adr/0005-dotnet-10-then-11.md) | Start on .NET 10, migrate to .NET 11 after release |
| [0006](adr/0006-media3-session-service-for-background-playback.md) | Media3 session service for background playback |
| [0007](adr/0007-typescript-player-scripts-and-message-contract.md) | TypeScript player scripts with a versioned message contract (ad skip messages changed by 0013) |
| [0008](adr/0008-branching-and-merge-strategy.md) | Branching and merge strategy |
| [0009](adr/0009-distribution-through-github-releases.md) | Distribution through GitHub Releases and Obtainium |
| [0010](adr/0010-test-libraries.md) | Test libraries |
| [0011](adr/0011-source-generated-mediator.md) | Dispatch messages with the source-generated Mediator library 3.0.2 |
| [0012](adr/0012-native-window-visibility-for-background-playback.md) | Keep background playback alive with a native window visibility override |
| [0013](adr/0013-native-tap-to-press-skip.md) | Press Skip with a native tap; the page only locates the button |
| [0014](adr/0014-androidx-alignment-and-android-6-minimum.md) | Pin AndroidX versions for Media3 and WebKit, and require Android 6.0 |
