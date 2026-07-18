# Contributing to OpenLogi

Thanks for your interest in OpenLogi! This guide covers how to build, test, and
contribute changes. Please also read [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
to understand the layout, and [`PLAN.md`](PLAN.md) for the product vision.

## Prerequisites

- The **.NET 8 SDK**. The exact version is pinned in
  [`global.json`](global.json); install a matching SDK so builds are
  reproducible.

## Build, test, run

```bash
# Restore + build the whole solution
dotnet build OpenLogi.sln

# Run all tests
dotnet test OpenLogi.sln

# Exercise the agent end-to-end without hardware
dotnet run --project src/OpenLogi.Agent -- --demo

# Launch the desktop UI
dotnet run --project src/OpenLogi.App
```

CI runs the same `restore` → `build` (Release) → `test` sequence on every push and
pull request (see [`.github/workflows/ci.yml`](.github/workflows/ci.yml)).

## Coding standards

- **Warnings are errors.** [`Directory.Build.props`](Directory.Build.props) sets
  `TreatWarningsAsErrors=true`, nullable reference types, and implicit usings for
  every project. Keep the build warning-clean. (The only exceptions are the
  unavoidable SQLite restore advisories `NU1903`/`NU1902`; see the architecture
  doc.)
- **Match the existing style.** The codebase uses immutable records for the domain
  model, XML doc comments on public types and members, and file-scoped namespaces.
- **Stay in the right layer.** Dependencies flow one way (see the architecture
  doc). Don't let device-specific logic leak above the device layer, and keep the
  HID layer free of business logic.
- **Keep the design capability-driven.** Gate behaviour on the capability graph,
  not on device models.
- **No telemetry, no network calls.** Offline-first and zero-telemetry are
  non-negotiable.

## Tests

- Tests use **xUnit** and live under `tests/`, one project per source layer.
- Prefer testing against the in-memory mocks and `OpenLogiDatabase.InMemory(...)`
  so tests need no hardware and no files on disk.
- Add or update tests alongside any behavioural change, and make sure
  `dotnet test OpenLogi.sln` passes before opening a pull request.

## Pull requests

- Keep changes focused and incremental.
- Ensure the build is warning-clean and all tests pass.
- Update documentation (`README.md`, `docs/ARCHITECTURE.md`) when your change
  affects the architecture, the developer workflow, or what is real vs. stubbed.
