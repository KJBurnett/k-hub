# OpenLogi

A lightweight, open-source, **offline-first, zero-telemetry** manager for Logitech
mice — a focused alternative to Logitech G HUB. OpenLogi aims to be fast,
native-feeling, and privacy-respecting: everything runs locally, nothing is sent
anywhere, and there is no account, no cloud, and no analytics.

See [`PLAN.md`](PLAN.md) for the full product and architecture brief. This
repository currently contains the **Phase 0 / Phase 1 foundation**: the full
architecture skeleton with genuine, tested implementations of the core layers and
a hardware-free demo mode. Real Windows HID communication is deliberately deferred
to a later phase (see [Roadmap](#roadmap)).

## Design principles

OpenLogi follows the philosophy set out in `PLAN.md`:

- **Capability-driven, not model-driven.** Behaviour is decided by what a device
  reports it can do, not by matching against a hard-coded model list. Unknown
  Logitech mice still work, degrading gracefully.
- **Offline-first & zero-telemetry.** No network calls, no telemetry — by design.
  Telemetry is not even a setting; it is always off.
- **Lightweight & native-feeling.** Fast startup and a small memory footprint are
  primary goals.
- **Layered & testable.** Every layer sits behind an interface, so the whole stack
  can be exercised without physical hardware.

## Stack

| Concern            | Choice                                              |
| ------------------ | --------------------------------------------------- |
| Language / runtime | C# on **.NET 8 (LTS)** (pinned via `global.json`)   |
| Desktop UI         | **Avalonia** (native-feeling, cross-platform ready) |
| Local storage      | **SQLite** (`Microsoft.Data.Sqlite`)                |
| Background agent    | `Microsoft.Extensions.Hosting` generic host        |
| Tests              | **xUnit**                                           |

Why .NET 8: it matches the C#-style interfaces described in `PLAN.md`, delivers
native performance and a small footprint, is an LTS release for reproducible CI,
and keeps a cross-platform future open through Avalonia.

## Repository layout

```
src/
  OpenLogi.Core/      Domain model: capabilities, devices, configuration, events
  OpenLogi.Logging/   Self-contained logging (console / file / in-memory sinks)
  OpenLogi.Storage/   SQLite schema + repositories (profiles, settings, app rules)
  OpenLogi.Hid/       HID transport abstraction + hardware-free mock backend
  OpenLogi.Devices/   Capability-driven IDevice, catalog, factory, device manager
  OpenLogi.Agent/     Background agent: DI composition root + hosted services
  OpenLogi.App/       Avalonia desktop UI shell
tests/
  OpenLogi.Core.Tests/
  OpenLogi.Hid.Tests/
  OpenLogi.Devices.Tests/
  OpenLogi.Storage.Tests/
```

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for how the layers fit together
and what is real vs. stubbed today.

## Getting started

Prerequisites: the **.NET 8 SDK** (the exact version is pinned in `global.json`).

```bash
# Restore and build the whole solution
dotnet build OpenLogi.sln

# Run all tests
dotnet test OpenLogi.sln
```

### Try the agent (no hardware required)

The background agent has a demo mode that seeds a mock Logitech mouse so you can
watch the full stack run end-to-end — device detection, default-profile creation,
and DPI / polling-rate application — without any physical device:

```bash
dotnet run --project src/OpenLogi.Agent -- --demo
```

The local database and logs are written under your platform's application data
directory (`%LOCALAPPDATA%/OpenLogi` on Windows, `~/.local/share/OpenLogi` on
Linux), never inside the repository.

### Run the desktop UI

```bash
dotnet run --project src/OpenLogi.App
```

The UI hosts the agent in-process and lists connected (mock) devices.

## Roadmap

- **Phase 0 / 1 (this repository):** stack selection, project scaffolding,
  CI, core architecture skeleton, capability model, logging, local storage,
  mock HID backend, demo mode, and contributor docs.
- **Phase 2:** real Windows HID backend implementing `IHidBackend`, swapped in
  behind the existing abstraction with no changes to the layers above.
- **Phase 3:** real Logitech HID++ 2.0 protocol replacing the provisional report
  framing in the device layer.

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md). In short: build with `dotnet build`,
test with `dotnet test`, and keep the code warning-clean — warnings are treated
as errors.

## License

See [`LICENSE`](LICENSE) if present; otherwise licensing will be finalized before
the first release.
