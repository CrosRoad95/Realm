namespace RealmCore.Server.Modules.Integrations;

public interface IDiscordIntegration : IIntegration
{
    ulong DiscordUserId { get; internal set; }

    string GenerateAndGetConnectionCode(TimeSpan? validFor = null);
    bool Verify(string code, ulong discordUserId);
}

internal sealed class DiscordIntegration : IDiscordIntegration
{
    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;

    public ulong DiscordUserId { get; set; }
    public RealmPlayer Player { get; }
    public DiscordIntegration(RealmPlayer player, IDateTimeProvider dateTimeProvider)
    {
        Player = player;
        _dateTimeProvider = dateTimeProvider;
    }

    private bool HasPendingDiscordConnectionCode()
    {
        lock (_lock)
            return _discordConnectionCodeValidUntil != null && _discordConnectionCodeValidUntil > _dateTimeProvider.Now;
    }

    public bool Verify(string code, ulong discordUserId)
    {
        ArgumentOutOfRangeException.ThrowIfZero(discordUserId);

        if (IsIntegrated())
            throw new InvalidOperationException();

        lock (_lock)
        {
            if (!HasPendingDiscordConnectionCode())
                return false;

            if (_discordConnectionCode == code)
            {
                DiscordUserId = discordUserId;
                return true;
            }
        }

        return false;
    }

    public string GenerateAndGetConnectionCode(TimeSpan? validFor = null)
    {
        if (IsIntegrated())
            throw new InvalidOperationException();

        lock (_lock)
        {
            _discordConnectionCode = Guid.NewGuid().ToString();
            _discordConnectionCodeValidUntil = _dateTimeProvider.Now.Add(validFor ?? TimeSpan.FromMinutes(2));
            return _discordConnectionCode;
        }
    }
    public bool IsIntegrated() => DiscordUserId != 0;

    public void Remove()
    {
        DiscordUserId = 0;
    }
}
