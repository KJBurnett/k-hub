# Architecture

This document explains how OpenLogi is structured, why, and what is real versus
stubbed today. It complements [`PLAN.md`](../PLAN.md), which is the authoritative
product and architecture brief.

## Layering

OpenLogi is a set of small, single-responsibility projects. Dependencies point in
one direction; nothing device-specific leaks upward.

```
        +----------------------+
        |     OpenLogi.App      |  Avalonia desktop UI
        +----------+-----------+
                   |
        +----------v-----------+
        |    OpenLogi.Agent     |  Background agent + DI composition root
        +----------+-----------+
                   |
        +----------v-----------+
        |   OpenLogi.Devices    |  Capability-driven IDevice, catalog, manager
        +----------+-----------+
                   |
        +----------v-----------+
        |     OpenLogi.Hid      |  HID transport abstraction (mock today)
        +----------------------+

   Cross-cutting (used by every layer above):
        OpenLogi.Core       Domain model + events
        OpenLogi.Logging    Logging facade + sinks
        OpenLogi.Storage    Local SQLite persistence
```

The `App` hosts the `Agent` in-process, so a single [composition
root](../src/OpenLogi.Agent/OpenLogiServices.cs) (`AddOpenLogi`) wires the entire
stack identically for both the headless agent and the desktop UI.

## Projects

### OpenLogi.Core

The domain model, with **no** dependencies on UI, storage, or hardware:

- **Capabilities** — `Capability` and the immutable `CapabilityGraph` (with a
  builder). This is the heart of the design: features are gated on capabilities,
  never on device models.
- **Devices** — `DeviceIdentity`, `DeviceInfo`, `DeviceTier`, `DeviceConnection`,
  `BatteryStatus`.
- **Configuration** — `Profile`, `DpiSettings`/`DpiStage`, `PollingRate`,
  `ButtonMapping`/`ButtonAction`, `Macro`, `ApplicationProfileRule`, `AppSettings`.
  These are immutable records; edits produce new instances.
- **Events** — `IEventBus` and a thread-safe synchronous `EventBus` for in-process
  domain events (device connected/disconnected, profile switched, etc.).

### OpenLogi.Logging

A small, self-contained logging facade (`IAppLogger` / `IAppLoggerFactory`) with
console, rolling-file, and in-memory sinks. The in-memory sink backs the future
diagnostics export. No third-party logging dependency is required.

### OpenLogi.Storage

Local, offline-first persistence on SQLite:

- `OpenLogiDatabase` owns a single long-lived connection and serialises access
  through `ExecuteAsync`, keeping startup fast. It offers `ForFile(...)` and an
  `InMemory(...)` factory used by tests.
- `Schema` holds versioned DDL for additive future migrations.
- Repositories: `ProfileRepository`, `SettingsRepository`,
  `ApplicationRuleRepository`.
- **Persistence DTOs** (`ProfileDto`) decouple the on-disk JSON shape from the
  domain model. This is deliberate: `System.Text.Json` cannot round-trip some
  domain records directly (e.g. `DpiSettings`, whose constructor parameter type
  differs from its property type), so the storage layer maps to/from DTOs and the
  Core model stays serializer-agnostic.

### OpenLogi.Hid

The single seam between OpenLogi and the operating system's HID stack:

- `IHidBackend` enumerates and opens devices and raises arrival/removal events.
- `IHidDevice` is an open connection (write/read output and feature reports).
- `Testing/MockHidBackend` and `Testing/MockHidDevice` provide an in-memory
  implementation for tests and demo mode.

The HID layer performs **raw communication only** and holds no business logic.

### OpenLogi.Devices

The capability-driven device layer:

- `IDevice` is the abstraction every mouse implements; everything above works
  purely against it.
- `GenericHidDevice` is a single implementation that serves all tiers. It contains
  **no model-specific branching** — every operation is gated on the capability
  graph. Reads return `Unknown`/`null` when unsupported; writes throw
  `CapabilityNotSupportedException`.
- `DeviceCatalog` maps a HID identity to a friendly name, support tier, and an
  initial capability set. It is only a hint: unknown Logitech devices fall back to
  a generic Tier 3 profile, and non-Logitech devices are reported as Unknown.
- `DeviceFactory` resolves catalog info, opens the transport, and wraps both in a
  `GenericHidDevice`.
- `DeviceManager` tracks connected devices, reacts to backend arrival/removal
  events, publishes domain events, and is designed to survive unplug / receiver
  reconnect / sleep-wake.

### OpenLogi.Agent

The background agent and the DI composition root:

- `OpenLogiServices.AddOpenLogi` registers every service (the whole dependency
  graph lives here).
- Hosted services: `StartupInitializer` (initialise the store, seed a default
  profile) then `AgentWorker` (start the device manager and profile switching).
- `ProfileSwitchingService` resolves the active profile from
  `ApplicationProfileRule`s; `IForegroundAppMonitor` abstracts foreground-app
  detection (a `Null` implementation is used until a platform monitor exists).

### OpenLogi.App

An Avalonia desktop shell (MVVM via `CommunityToolkit.Mvvm`) that hosts the agent
in-process and lists connected devices, updating live from the `EventBus`.

## What's real vs. stubbed

Real and tested today:

- The capability model, configuration model, and event bus.
- Logging and its sinks.
- SQLite storage and all repositories (round-trip tested against an in-memory DB).
- The device layer's capability gating, catalog resolution, and device manager
  lifecycle — exercised against the mock HID backend.
- The agent's startup and wiring, verified end-to-end via `--demo`.

Deliberately stubbed / provisional:

- **HID backend** — only the in-memory mock exists. The real Windows backend
  (`IHidBackend`) lands in **Phase 2** and swaps in behind the abstraction with no
  changes above it.
- **Device report framing** (`DeviceReports`) — the byte layouts are placeholders,
  **not** the real Logitech HID++ protocol. Real HID++ 2.0 arrives in **Phase 3**.
- **Foreground-app monitor** — `NullForegroundAppMonitor` until a platform
  implementation is added.

This keeps the implementation honest: everything present is genuinely testable,
and nothing pretends to talk to hardware it cannot yet reach.

## Dependency & security notes

- The runtime and toolchain are pinned in [`global.json`](../global.json), and
  shared build settings live in
  [`Directory.Build.props`](../Directory.Build.props): `net8.0`, nullable
  reference types, implicit usings, analyzers on, and **warnings treated as
  errors**.
- **SQLite native advisory:** the SQLite native bundle
  (`SQLitePCLRaw.lib.e_sqlite3`) has an open advisory
  (`GHSA-2m69-gcr7-jv3q`) with no patched version available upstream. Because
  `PLAN.md` mandates SQLite for local storage, this dependency is unavoidable. It
  is mitigated by pinning the newest published bundle, isolating all SQLite access
  behind the Storage layer, and excluding only the corresponding restore
  advisories (`NU1903`/`NU1902`) from the warnings-as-errors policy — every other
  warning still fails the build. This should be revisited whenever an upstream fix
  ships.
