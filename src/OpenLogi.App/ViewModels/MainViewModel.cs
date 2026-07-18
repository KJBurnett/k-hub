using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenLogi.Core.Events;
using OpenLogi.Devices;

namespace OpenLogi.App.ViewModels;

/// <summary>
/// The main window view model. It shows the devices the background agent has
/// discovered and reacts to connect / disconnect events so the list stays live.
/// The UI itself is deliberately minimal and fast (PLAN.md section 25).
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly DeviceManager? _deviceManager;

    [ObservableProperty]
    private string _statusText = "Starting…";

    /// <summary>Design-time / fallback constructor with no live services.</summary>
    public MainViewModel()
    {
        Devices = new ObservableCollection<DeviceListItem>();
        StatusText = "No agent connected (design mode).";
    }

    /// <summary>Runtime constructor wired to the live device manager and event bus.</summary>
    public MainViewModel(DeviceManager deviceManager, IEventBus eventBus)
    {
        ArgumentNullException.ThrowIfNull(deviceManager);
        ArgumentNullException.ThrowIfNull(eventBus);
        _deviceManager = deviceManager;
        Devices = new ObservableCollection<DeviceListItem>();

        eventBus.Subscribe<DeviceConnectedEvent>(_ => RefreshOnUiThread());
        eventBus.Subscribe<DeviceDisconnectedEvent>(_ => RefreshOnUiThread());

        Refresh();
    }

    /// <summary>The currently connected devices.</summary>
    public ObservableCollection<DeviceListItem> Devices { get; }

    private void RefreshOnUiThread()
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            Refresh();
        }
        else
        {
            Dispatcher.UIThread.Post(Refresh);
        }
    }

    private void Refresh()
    {
        Devices.Clear();
        if (_deviceManager is null)
        {
            return;
        }

        foreach (var device in _deviceManager.ConnectedDevices)
        {
            Devices.Add(new DeviceListItem(device.Info));
        }

        StatusText = Devices.Count switch
        {
            0 => "No devices detected. Connect a Logitech mouse to begin.",
            1 => "1 device connected.",
            _ => $"{Devices.Count} devices connected.",
        };
    }
}
