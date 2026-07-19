using Microsoft.Data.Sqlite;
using OpenLogi.Logging;

namespace OpenLogi.Storage;

/// <summary>
/// Owns the single SQLite connection to the local OpenLogi database and applies
/// the schema. Access is serialised through <see cref="ExecuteAsync{T}"/> so the
/// connection can be shared safely across the agent and UI threads. Using one
/// long-lived connection keeps startup fast (PLAN.md section 21) and supports an
/// in-memory database for tests.
/// </summary>
public sealed class OpenLogiDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly object _lifecycleGate = new();
    private readonly IAppLogger _logger;
    private bool _initialized;
    private bool _disposed;
    private int _activeOperations;
    private TaskCompletionSource? _operationsDrained;
    private Task? _disposeTask;

    /// <summary>Creates a database over an explicit connection string.</summary>
    public OpenLogiDatabase(string connectionString, IAppLoggerFactory loggerFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _connection = new SqliteConnection(connectionString);
        _logger = loggerFactory.CreateLogger<OpenLogiDatabase>();
    }

    /// <summary>
    /// Creates a database backed by a file at <paramref name="path"/>, ensuring
    /// the containing directory exists.
    /// </summary>
    public static OpenLogiDatabase ForFile(string path, IAppLoggerFactory loggerFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new OpenLogiDatabase($"Data Source={path}", loggerFactory);
    }

    /// <summary>Creates a private in-memory database, primarily for tests.</summary>
    public static OpenLogiDatabase InMemory(IAppLoggerFactory loggerFactory)
        => new("Data Source=:memory:", loggerFactory);

    /// <summary>Opens the connection and applies the schema. Safe to call repeatedly.</summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        EnterOperation();
        try
        {
            await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_initialized)
                {
                    return;
                }

                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                await ExecutePragmaAsync("PRAGMA foreign_keys = ON;", cancellationToken).ConfigureAwait(false);
                await ExecutePragmaAsync("PRAGMA journal_mode = WAL;", cancellationToken).ConfigureAwait(false);

                await using (var command = _connection.CreateCommand())
                {
                    command.CommandText = Schema.CreateScript;
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await ApplyVersionAsync(cancellationToken).ConfigureAwait(false);
                _initialized = true;
                _logger.Information($"Local database initialised (schema v{Schema.Version}).");
            }
            finally
            {
                _mutex.Release();
            }
        }
        finally
        {
            ExitOperation();
        }
    }

    /// <summary>
    /// Runs <paramref name="action"/> against the connection while holding the
    /// access lock, guaranteeing serialised database access.
    /// </summary>
    public async Task<T> ExecuteAsync<T>(
        Func<SqliteConnection, Task<T>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        EnsureInitialized();
        EnterOperation();
        try
        {
            await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await action(_connection).ConfigureAwait(false);
            }
            finally
            {
                _mutex.Release();
            }
        }
        finally
        {
            ExitOperation();
        }
    }

    /// <summary>Runs a database action that returns no value.</summary>
    public Task ExecuteAsync(
        Func<SqliteConnection, Task> action, CancellationToken cancellationToken = default)
        => ExecuteAsync(async conn =>
        {
            await action(conn).ConfigureAwait(false);
            return true;
        }, cancellationToken);

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        lock (_lifecycleGate)
        {
            if (_disposeTask is null)
            {
                _disposed = true;
                _disposeTask = DisposeAfterOperationsAsync(
                    _activeOperations == 0
                        ? Task.CompletedTask
                        : (_operationsDrained = new TaskCompletionSource(
                            TaskCreationOptions.RunContinuationsAsynchronously)).Task);
            }

            return new ValueTask(_disposeTask);
        }
    }

    private async Task DisposeAfterOperationsAsync(Task operationsDrained)
    {
        await operationsDrained.ConfigureAwait(false);
        await _mutex.WaitAsync().ConfigureAwait(false);
        try
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _mutex.Release();
        }

        _mutex.Dispose();
    }

    private async Task ExecutePragmaAsync(string pragma, CancellationToken cancellationToken)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = pragma;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyVersionAsync(CancellationToken cancellationToken)
    {
        await using var read = _connection.CreateCommand();
        read.CommandText = "SELECT version FROM schema_info LIMIT 1;";
        var existing = await read.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            await using var insert = _connection.CreateCommand();
            insert.CommandText = "INSERT INTO schema_info (version) VALUES ($v);";
            insert.Parameters.AddWithValue("$v", Schema.Version);
            await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private void EnsureInitialized()
    {
        ThrowIfDisposed();
        if (!_initialized)
        {
            throw new InvalidOperationException(
                "The database must be initialised before use. Call InitializeAsync first.");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void EnterOperation()
    {
        lock (_lifecycleGate)
        {
            ThrowIfDisposed();
            _activeOperations++;
        }
    }

    private void ExitOperation()
    {
        lock (_lifecycleGate)
        {
            _activeOperations--;
            if (_activeOperations == 0 && _disposed)
            {
                _operationsDrained?.TrySetResult();
            }
        }
    }
}
