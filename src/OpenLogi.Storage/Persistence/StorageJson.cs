using System.Text.Json;

namespace OpenLogi.Storage.Persistence;

/// <summary>Shared JSON options for persisted payloads.</summary>
internal static class StorageJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
