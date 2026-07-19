namespace OpenLogi.Storage;

/// <summary>
/// The SQLite schema for OpenLogi's local, offline-first store (PLAN.md
/// section 15). The schema is versioned so future phases can migrate it
/// additively. All data stays on the user's machine.
/// </summary>
internal static class Schema
{
    /// <summary>Current schema version written to <c>schema_info</c>.</summary>
    public const int Version = 1;

    /// <summary>DDL that creates every table if it does not already exist.</summary>
    public const string CreateScript = """
        CREATE TABLE IF NOT EXISTS schema_info (
            version INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS settings (
            key   TEXT PRIMARY KEY,
            value TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS devices (
            stable_key    TEXT PRIMARY KEY,
            vendor_id     INTEGER NOT NULL,
            product_id    INTEGER NOT NULL,
            serial        TEXT,
            name          TEXT NOT NULL,
            tier          INTEGER NOT NULL,
            last_seen_utc TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS profiles (
            id           TEXT PRIMARY KEY,
            name         TEXT NOT NULL,
            is_default   INTEGER NOT NULL,
            payload_json TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS applications (
            executable TEXT PRIMARY KEY,
            profile_id TEXT NOT NULL,
            FOREIGN KEY (profile_id) REFERENCES profiles (id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS macros (
            id           TEXT PRIMARY KEY,
            name         TEXT NOT NULL,
            repeat_count INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS macro_steps (
            id       INTEGER PRIMARY KEY AUTOINCREMENT,
            macro_id TEXT NOT NULL,
            ordinal  INTEGER NOT NULL,
            kind     INTEGER NOT NULL,
            value    TEXT,
            delay_ms INTEGER NOT NULL,
            FOREIGN KEY (macro_id) REFERENCES macros (id) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS diagnostics (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            created_utc TEXT NOT NULL,
            category    TEXT NOT NULL,
            detail      TEXT NOT NULL
        );
        """;
}
