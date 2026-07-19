using Microsoft.Extensions.Hosting;
using OpenLogi.Agent;

// Standalone entry point for the background agent. The agent normally runs
// silently (PLAN.md section 12). Passing "--demo" seeds a mock device so the
// agent can be exercised without hardware or a UI.
var demo = args.Contains("--demo", StringComparer.OrdinalIgnoreCase);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddOpenLogi(options => options.UseDemoDevices = demo);

using var host = builder.Build();
await host.RunAsync();
