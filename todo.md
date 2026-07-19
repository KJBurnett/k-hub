# High-priority engineering backlog

These items were verified against the current source on 2026-07-19. They are
ordered by their ability to cause incorrect hardware behavior, data loss, or an
unusable release. None should be considered fixed merely because the mock demo
passes.

1. **Implement the real Logitech HID++ 2.0 protocol (critical).** `DeviceReports`
   explicitly emits provisional bytes, so the application cannot safely read or
   configure a physical mouse. Evidence:
   `src/OpenLogi.Devices/DeviceReports.cs:3-8`.
2. **Add Windows device arrival/removal monitoring (critical).** The Phase 2 backend
   enumerates current devices but cannot publish `DeviceArrived` or `DeviceRemoved`;
   plugged, unplugged, sleeping, and reconnecting mice will not update live.
   Evidence: `src/OpenLogi.Hid/WindowsHidBackend.cs:31-40`.
3. **Implement device connection classification (high).** Windows descriptors do
   not identify wired, receiver, Bluetooth, or Lightspeed connections, making the
   device layer choose inaccurate baseline capabilities. Evidence:
   `src/OpenLogi.Hid/HidDeviceDescriptor.cs:10-38`,
   `src/OpenLogi.Devices/DeviceCatalog.cs:86-90`.
4. **Validate the catalog IDs against physical devices (high).** The listed product
   IDs are documented placeholders and can assign the wrong name, tier, and
   capabilities. Evidence: `src/OpenLogi.Devices/DeviceCatalog.cs:50-52`.
5. **Implement the Windows foreground-application monitor (high).** The registered
   monitor is a no-op, so automatic per-application profile switching cannot work.
   Evidence: `src/OpenLogi.Agent/OpenLogiServices.cs:69-70`,
   `src/OpenLogi.Agent/Services/NullForegroundAppMonitor.cs`.
6. **Implement a macro execution engine (high).** Macros are persisted domain data
   only; no component executes them or sends input safely. Evidence:
   `src/OpenLogi.Core/Configuration/Macro.cs`, `PLAN.md:906-918`.
7. **Add configuration UI for DPI, polling, buttons, profiles, and macros (high).**
   The desktop application only presents a device list, leaving users unable to
   configure the functionality in the domain model. Evidence:
   `src/OpenLogi.App/Views/MainWindow.axaml`,
   `src/OpenLogi.App/ViewModels/MainViewModel.cs`.
8. **Track and drain asynchronous event work during shutdown (high).** Device and
   foreground event handlers launch work with `CancellationToken.None`; operations
   can outlive host shutdown and access disposed state. Evidence:
   `src/OpenLogi.Devices/DeviceManager.cs:65-68`,
   `src/OpenLogi.Agent/Services/AgentWorker.cs:65-71`.
9. **Make database initialization concurrency-safe (high).** Concurrent
   `InitializeAsync` callers can both observe `_initialized == false` and open or
   initialize the same SQLite connection twice. Evidence:
   `src/OpenLogi.Storage/OpenLogiDatabase.cs:50-70`.
10. **Guarantee database and log-sink disposal on host shutdown (high).**
    `OpenLogiDatabase` only implements `IAsyncDisposable`, while `FileLogSink` is
    constructed inline and is not DI-owned; the current lifecycle can retain file
    handles and leave SQLite state unflushed. Evidence:
    `src/OpenLogi.Storage/OpenLogiDatabase.cs:13,102-107`,
    `src/OpenLogi.Agent/OpenLogiServices.cs:35-41`.
11. **Validate persisted profile DTOs before applying them (high).** Deserialized
    payload fields are converted to domain values without an explicit storage
    validation step, so a corrupt local row can load invalid configuration or throw
    at startup. Evidence:
    `src/OpenLogi.Storage/Repositories/ProfileRepository.cs:122-127`,
    `src/OpenLogi.Storage/Persistence/ProfileDto.cs`.
12. **Add physical-device integration coverage (high).** Current HID tests exercise
    only `MockHidBackend`; the Windows API path and each Tier 1 device have no smoke,
    reconnect, sleep, battery, or protocol regression tests. Evidence:
    `tests/OpenLogi.Hid.Tests/MockHidBackendTests.cs`, `PLAN.md:662-698`.
