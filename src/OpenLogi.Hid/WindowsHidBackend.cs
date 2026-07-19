using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using OpenLogi.Core.Devices;

namespace OpenLogi.Hid;

/// <summary>
/// Enumerates and opens HID devices through the Windows HID and SetupAPI APIs.
/// Device-arrival notifications are intentionally left to the host integration;
/// callers can always refresh the current device list through <see cref="Enumerate"/>.
/// </summary>
public sealed class WindowsHidBackend : IHidBackend
{
    private const uint DigcfPresent = 0x00000002;
    private const uint DigcfDeviceInterface = 0x00000010;
    private const uint FileFlagOverlapped = 0x40000000;
    private const int HidpStatusSuccess = 0x00110000;
    private const int ErrorNoMoreItems = 259;
    private const int ErrorInsufficientBuffer = 122;

    /// <summary>Creates a Windows HID backend.</summary>
    /// <exception cref="PlatformNotSupportedException">When not running on Windows.</exception>
    public WindowsHidBackend()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows HID support is only available on Windows.");
        }
    }

    /// <inheritdoc />
    public event EventHandler<HidDeviceEventArgs>? DeviceArrived
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public event EventHandler<HidDeviceEventArgs>? DeviceRemoved
    {
        add { }
        remove { }
    }

    /// <inheritdoc />
    public IReadOnlyList<HidDeviceDescriptor> Enumerate()
    {
        HidD_GetHidGuid(out var hidGuid);
        using var deviceInfoSet = SetupDiGetClassDevs(
            ref hidGuid, null, IntPtr.Zero, DigcfPresent | DigcfDeviceInterface);
        if (deviceInfoSet.IsInvalid)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to enumerate HID devices.");
        }

        var descriptors = new List<HidDeviceDescriptor>();
        for (uint index = 0; ; index++)
        {
            var interfaceData = new SpDeviceInterfaceData
            {
                CbSize = Marshal.SizeOf<SpDeviceInterfaceData>(),
            };

            if (!SetupDiEnumDeviceInterfaces(
                deviceInfoSet, IntPtr.Zero, ref hidGuid, index, ref interfaceData))
            {
                var error = Marshal.GetLastWin32Error();
                if (error == ErrorNoMoreItems)
                {
                    break;
                }

                throw new Win32Exception(error, "Unable to enumerate a HID device interface.");
            }

            descriptors.Add(ReadDescriptor(deviceInfoSet, interfaceData));
        }

        return descriptors;
    }

    /// <inheritdoc />
    public IHidDevice Open(HidDeviceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        return new WindowsHidDevice(descriptor);
    }

    private static HidDeviceDescriptor ReadDescriptor(
        SafeDeviceInfoSetHandle deviceInfoSet, SpDeviceInterfaceData interfaceData)
    {
        SetupDiGetDeviceInterfaceDetail(
            deviceInfoSet, ref interfaceData, IntPtr.Zero, 0, out var requiredSize, IntPtr.Zero);
        var detailError = Marshal.GetLastWin32Error();
        if (requiredSize == 0 || detailError != ErrorInsufficientBuffer)
        {
            throw new Win32Exception(detailError, "Unable to determine the HID device path length.");
        }

        // SetupAPI determines this variable-sized structure at runtime, so it
        // cannot be represented by a fixed managed or stack allocation.
        var detail = Marshal.AllocHGlobal(checked((int)requiredSize));
        try
        {
            Marshal.WriteInt32(detail, IntPtr.Size == 8 ? 8 : 6);
            if (!SetupDiGetDeviceInterfaceDetail(
                deviceInfoSet, ref interfaceData, detail, requiredSize, out _, IntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to read the HID device path.");
            }

            var path = Marshal.PtrToStringUni(IntPtr.Add(detail, 4))
                ?? throw new InvalidOperationException("Windows returned an empty HID device path.");
            return InspectDevice(path);
        }
        finally
        {
            Marshal.FreeHGlobal(detail);
        }
    }

    private static HidDeviceDescriptor InspectDevice(string path)
    {
        using var handle = CreateFile(
            path, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open,
            (uint)FileAttributes.Normal | FileFlagOverlapped, IntPtr.Zero);
        if (handle.IsInvalid)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to inspect HID device '{path}'.");
        }

        var attributes = new HiddAttributes { Size = Marshal.SizeOf<HiddAttributes>() };
        if (!HidD_GetAttributes(handle, ref attributes))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to read HID attributes for '{path}'.");
        }

        if (!HidD_GetPreparsedData(handle, out var preparsedData))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Unable to read HID capabilities for '{path}'.");
        }

        try
        {
            var status = HidP_GetCaps(preparsedData, out var caps);
            if (status != HidpStatusSuccess)
            {
                throw new InvalidOperationException($"Unable to read HID capabilities for '{path}' (status 0x{status:X8}).");
            }

            return new HidDeviceDescriptor
            {
                Identity = new DeviceIdentity(attributes.VendorId, attributes.ProductId),
                Path = path,
                UsagePage = caps.UsagePage,
                Usage = caps.Usage,
                InputReportLength = caps.InputReportByteLength,
                OutputReportLength = caps.OutputReportByteLength,
                FeatureReportLength = caps.FeatureReportByteLength,
            };
        }
        finally
        {
            HidD_FreePreparsedData(preparsedData);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SpDeviceInterfaceData
    {
        public int CbSize;
        public Guid InterfaceClassGuid;
        public int Flags;
        public IntPtr Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HiddAttributes
    {
        public int Size;
        public ushort VendorId;
        public ushort ProductId;
        public ushort VersionNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HidpCaps
    {
        public ushort Usage;
        public ushort UsagePage;
        public ushort InputReportByteLength;
        public ushort OutputReportByteLength;
        public ushort FeatureReportByteLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public ushort[] Reserved;
        public ushort NumberLinkCollectionNodes;
        public ushort NumberInputButtonCaps;
        public ushort NumberInputValueCaps;
        public ushort NumberInputDataIndices;
        public ushort NumberOutputButtonCaps;
        public ushort NumberOutputValueCaps;
        public ushort NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps;
        public ushort NumberFeatureValueCaps;
        public ushort NumberFeatureDataIndices;
    }

    [DllImport("hid.dll")]
    private static extern void HidD_GetHidGuid(out Guid hidGuid);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetAttributes(SafeFileHandle handle, ref HiddAttributes attributes);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetPreparsedData(SafeFileHandle handle, out IntPtr preparsedData);

    [DllImport("hid.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

    [DllImport("hid.dll")]
    private static extern int HidP_GetCaps(IntPtr preparsedData, out HidpCaps capabilities);

    [DllImport("setupapi.dll", SetLastError = true)]
    private static extern SafeDeviceInfoSetHandle SetupDiGetClassDevs(
        ref Guid classGuid, string? enumerator, IntPtr parent, uint flags);

    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiEnumDeviceInterfaces(
        SafeDeviceInfoSetHandle deviceInfoSet,
        IntPtr deviceInfoData,
        ref Guid interfaceClassGuid,
        uint memberIndex,
        ref SpDeviceInterfaceData deviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiGetDeviceInterfaceDetail(
        SafeDeviceInfoSetHandle deviceInfoSet,
        ref SpDeviceInterfaceData deviceInterfaceData,
        IntPtr deviceInterfaceDetailData,
        uint deviceInterfaceDetailDataSize,
        out uint requiredSize,
        IntPtr deviceInfoData);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string fileName,
        FileAccess desiredAccess,
        FileShare shareMode,
        IntPtr securityAttributes,
        FileMode creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    private sealed class SafeDeviceInfoSetHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDeviceInfoSetHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle() => SetupDiDestroyDeviceInfoList(handle);
    }

    [DllImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);
}
