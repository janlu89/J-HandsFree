# Contributing

Thanks for your interest. J-HandsFree is a personal project, but the process below applies to every change, including the owner's.

## Before you start

1. Read [ARCHITECTURE.md](docs/architecture/ARCHITECTURE.md) and [CODING_STANDARDS.md](docs/CODING_STANDARDS.md).
2. Open an issue describing the change, unless it is a typo fix.

## Workflow

1. Branch from `dev`:

   ```powershell
   git switch dev
   git pull
   git switch -c feature/short-description
   ```

   | Prefix | Use for |
   | --- | --- |
   | `feature/` | New behaviour |
   | `fix/` | Bug fixes |
   | `refactor/` | Code changes without behaviour change |
   | `test/` | Tests only |
   | `docs/` | Documentation only |
   | `chore/` | Build, tooling, dependencies |
   | `ci/` | Workflow changes |

2. Commit with [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/): `type(scope): description`.
3. Run the checks locally (see below).
4. Open a pull request into `dev`. The pull request title must be a Conventional Commit, because it becomes the squashed commit message.
5. All checks must pass. The pull request is squash-merged and the branch is deleted.

Full details: [GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md).

## Local checks

```powershell
dotnet format JHandsFree.slnx --verify-no-changes
dotnet build JHandsFree.slnx -c Release
dotnet test JHandsFree.slnx -c Release
npm --prefix src/player-scripts run lint
npm --prefix src/player-scripts test
npm --prefix src/player-scripts run build
```

## Definition of done for a pull request

- [ ] The change does one thing, described in the title.
- [ ] Code follows [CODING_STANDARDS.md](docs/CODING_STANDARDS.md).
- [ ] New behaviour is covered by tests that fail without the change.
- [ ] All CI checks pass.
- [ ] Documentation and `CHANGELOG.md` are updated when behaviour changes.
- [ ] An ADR is added when an architectural rule changes.
