using OpenLogi.Core.Configuration;
using OpenLogi.Core.Events;
using OpenLogi.Devices;
using OpenLogi.Logging;
using OpenLogi.Storage.Repositories;

namespace OpenLogi.Agent.Services;

/// <summary>
/// Resolves the profile that should be active for the current foreground
/// application and applies it to every connected device (PLAN.md sections 9
/// and 12, "Automatic profile switching"). Resolution is purely rule based, so
/// it is fully testable without hardware.
/// </summary>
public sealed class ProfileSwitchingService
{
    private readonly IApplicationRuleRepository _rules;
    private readonly IProfileRepository _profiles;
    private readonly DeviceManager _devices;
    private readonly IEventBus _eventBus;
    private readonly IAppLogger _logger;

    /// <summary>Creates the service.</summary>
    public ProfileSwitchingService(
        IApplicationRuleRepository rules,
        IProfileRepository profiles,
        DeviceManager devices,
        IEventBus eventBus,
        IAppLoggerFactory loggerFactory)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        _devices = devices ?? throw new ArgumentNullException(nameof(devices));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<ProfileSwitchingService>();
    }

    /// <summary>
    /// Resolves the profile for <paramref name="foregroundExecutable"/>: the
    /// profile referenced by a matching application rule, otherwise the default
    /// profile, otherwise null.
    /// </summary>
    public async Task<Profile?> ResolveProfileAsync(
        string? foregroundExecutable, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(foregroundExecutable))
        {
            var rules = await _rules.GetAllAsync(cancellationToken).ConfigureAwait(false);
            var match = rules.FirstOrDefault(r => r.Matches(foregroundExecutable));
            if (match is not null)
            {
                var mapped = await _profiles.GetAsync(match.ProfileId, cancellationToken).ConfigureAwait(false);
                if (mapped is not null)
                {
                    return mapped;
                }
            }
        }

        return await _profiles.GetDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves and applies the appropriate profile for the current foreground
    /// application to every connected device.
    /// </summary>
    public async Task ApplyForForegroundAsync(
        string? foregroundExecutable, CancellationToken cancellationToken = default)
    {
        var profile = await ResolveProfileAsync(foregroundExecutable, cancellationToken).ConfigureAwait(false);
        if (profile is null)
        {
            _logger.Debug("No profile resolved for the current foreground application.");
            return;
        }

        foreach (var device in _devices.ConnectedDevices)
        {
            try
            {
                await device.ApplySettingsAsync(profile, cancellationToken).ConfigureAwait(false);
                _eventBus.Publish(new ProfileSwitchedEvent(device.Info.Identity.StableKey, profile.Id));
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to apply profile '{profile.Name}' to {device.Info.Name}.", ex);
            }
        }

        _logger.Information(
            $"Applied profile '{profile.Name}' for foreground '{foregroundExecutable ?? "(none)"}'.");
    }
}
