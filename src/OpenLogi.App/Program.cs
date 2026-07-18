using System;
using System.Linq;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenLogi.Agent;
using OpenLogi.App.ViewModels;

namespace OpenLogi.App;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't
    // initialized yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // The UI hosts the background agent in-process (PLAN.md section 11:
        // UI -> Background Agent -> Core -> ...). A "--demo" flag seeds a mock
        // device so the UI is useful without hardware during early phases.
        var demo = args.Contains("--demo", StringComparer.OrdinalIgnoreCase);

        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddOpenLogi(options => options.UseDemoDevices = demo);
        builder.Services.AddSingleton<MainViewModel>();

        using var host = builder.Build();
        host.Start();
        App.Services = host.Services;

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            host.StopAsync().GetAwaiter().GetResult();
        }
    }

    // Avalonia configuration, don't remove; also used by the visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
