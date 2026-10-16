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
| [0002](adr/0002-host-youtube-mobile-site-in-webview.md) | Host the YouTube mobile website in a WebView |
| [0003](adr/0003-clean-architecture-without-cqrs.md) | Clean Architecture with ports and adapters, without CQRS (mediator part superseded by 0011) |
| [0004](adr/0004-no-backend.md) | No backend |
| [0005](adr/0005-dotnet-10-then-11.md) | Start on .NET 10, migrate to .NET 11 after release |
| [0006](adr/0006-media3-session-service-for-background-playback.md) | Media3 session service for background playback |
| [0007](adr/0007-typescript-player-scripts-and-message-contract.md) | TypeScript player scripts with a versioned message contract |
| [0008](adr/0008-branching-and-merge-strategy.md) | Branching and merge strategy |
| [0009](adr/0009-distribution-through-github-releases.md) | Distribution through GitHub Releases and Obtainium |
| [0010](adr/0010-test-libraries.md) | Test libraries |
| [0011](adr/0011-source-generated-mediator.md) | Dispatch messages with the source-generated Mediator library 3.0.2 |
