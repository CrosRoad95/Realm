namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class PendingDiscordIntegrationComponent : Component
{
    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;

    public PendingDiscordIntegrationComponent(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    private bool HasPendingDiscordConnectionCode()
    {
        lock (_lock)
            return _discordConnectionCodeValidUntil != null && _discordConnectionCodeValidUntil > _dateTimeProvider.Now;
    }

    public bool Verify(string code)
    {
        lock (_lock)
        {
            if (!HasPendingDiscordConnectionCode())
                return false;

            return _discordConnectionCode == code;
        }
    }

    public string GenerateAndGetDiscordConnectionCode(TimeSpan? validFor = null)
    {
        lock (_lock)
        {
            _discordConnectionCode = Guid.NewGuid().ToString();
            _discordConnectionCodeValidUntil = _dateTimeProvider.Now.Add(validFor ?? TimeSpan.FromMinutes(2));
            return _discordConnectionCode;
        }
    }
}
