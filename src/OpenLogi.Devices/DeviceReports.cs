namespace OpenLogi.Devices;

/// <summary>
/// Provisional report framing used to wire the device layer end-to-end and make
/// it testable against the mock HID backend. These byte layouts are placeholders
/// and are NOT the real Logitech HID++ protocol; they will be replaced by a
/// proper HID++ 2.0 implementation during Phase 3 (PLAN.md sections 27, "Phase 3"
/// and Appendix A #4 — keep transport separate from business logic).
/// </summary>
internal static class DeviceReports
{
    // HID++ uses report ids 0x10 (short) and 0x11 (long); we reuse 0x11 as a
    // stand-in so the framing resembles the eventual real protocol.
    public const byte LongReportId = 0x11;

    public const byte SetDpiCommand = 0x01;
    public const byte SetPollingRateCommand = 0x02;
    public const byte MapButtonCommand = 0x03;

    public static byte[] SetDpi(int dpi) =>
        new[] { LongReportId, SetDpiCommand, (byte)(dpi & 0xFF), (byte)((dpi >> 8) & 0xFF) };

    public static byte[] SetPollingRate(int hz)
    {
        // Encode as the divisor of 8000 Hz where it divides evenly (1000->8),
        // otherwise fall back to the raw low byte of the rate.
        var code = hz > 0 && 8000 % hz == 0 ? (byte)(8000 / hz) : (byte)(hz & 0xFF);
        return new[] { LongReportId, SetPollingRateCommand, code };
    }

    public static byte[] MapButton(int buttonId, byte actionKind) =>
        new[] { LongReportId, MapButtonCommand, (byte)(buttonId & 0xFF), actionKind };
}
