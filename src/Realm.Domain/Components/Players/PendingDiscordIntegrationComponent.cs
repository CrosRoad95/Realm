using Realm.Common.Providers;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class PendingDiscordIntegrationComponent : Component
{
    [Inject]
    protected IDateTimeProvider DateTimeProvider { get; set; } = default!;

    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;
    private object _lock = new();

    public PendingDiscordIntegrationComponent()
    {
    }

    private bool HasPendingDiscordConnectionCode() {
        ThrowIfDisposed();

        lock (_lock)
            return _discordConnectionCodeValidUntil != null && _discordConnectionCodeValidUntil > DateTimeProvider.Now;
    }

    public bool Verify(string code)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (!HasPendingDiscordConnectionCode())
            return false;

            return _discordConnectionCode == code;
        }
    }

    public string GenerateAndGetDiscordConnectionCode(TimeSpan? validFor = null)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            _discordConnectionCode = Guid.NewGuid().ToString();
            _discordConnectionCodeValidUntil = DateTimeProvider.Now.Add(validFor ?? TimeSpan.FromMinutes(2));
            return _discordConnectionCode;
        }
    }
}
