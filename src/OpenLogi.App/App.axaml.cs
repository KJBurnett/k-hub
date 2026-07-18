using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using OpenLogi.App.ViewModels;
using OpenLogi.App.Views;

namespace OpenLogi.App;

public partial class App : Application
{
    /// <summary>
    /// The application service provider, set by <see cref="Program"/> once the
    /// in-process host has started. View models are resolved from here.
    /// </summary>
    public static IServiceProvider? Services { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = Services?.GetService<MainViewModel>() ?? new MainViewModel();
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
