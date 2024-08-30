namespace RealmCore.Server.Modules.Integrations;

public interface IDiscordIntegration : IIntegration
{
    ulong DiscordUserId { get; }
    string? Name { get; }

    bool GenerateAndGetConnectionCode(out string code, TimeSpan? validFor = null);
    void Integrate(ulong discordUserId, string name, bool isNew = false);
    bool TryVerify(string code, ulong discordUserId, string name);
}

internal sealed class DiscordIntegration : IDiscordIntegration
{
    private readonly object _lock = new();
    private string? _discordConnectionCode = null;
    private DateTime? _discordConnectionCodeValidUntil = null;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ulong DiscordUserId { get; private set; }
    public string? Name { get; private set; }

    public event Action<IIntegration>? Created;
    public event Action<IIntegration>? Removed;

    public RealmPlayer Player { get; }
    public DiscordIntegration(RealmPlayer player, IDateTimeProvider dateTimeProvider)
    {
        Player = player;
        _dateTimeProvider = dateTimeProvider;
    }

    public void Integrate(ulong discordUserId, string name, bool isNew = false)
    {
        lock (_lock)
        {
            if (IsIntegrated())
                return;

            DiscordUserId = discordUserId;
            Name = name;
            if(isNew)
                Created?.Invoke(this);
        }
    }

    private bool HasPendingDiscordConnectionCode()
    {
        lock (_lock)
        {
            if (IsIntegrated())
                return false;

            return _discordConnectionCodeValidUntil != null && _discordConnectionCodeValidUntil > _dateTimeProvider.Now;
        }
    }

    public bool TryVerify(string code, ulong discordUserId, string name)
    {
        ArgumentOutOfRangeException.ThrowIfZero(discordUserId);

        lock (_lock)
        {
            if (IsIntegrated())
                return false;

            if (!HasPendingDiscordConnectionCode())
                return false;

            if (_discordConnectionCode == code)
            {
                Integrate(discordUserId, name, true);
                return true;
            }
        }

        return false;
    }

    public bool GenerateAndGetConnectionCode(out string code, TimeSpan? validFor = null)
    {
        lock (_lock)
        {
            if (IsIntegrated())
            {
                code = "";
                return false;
            }

            _discordConnectionCode = Guid.NewGuid().ToString();
            _discordConnectionCodeValidUntil = _dateTimeProvider.Now.Add(validFor ?? TimeSpan.FromMinutes(2));
            code = _discordConnectionCode;
            return true;
        }
    }

    public bool IsIntegrated() => DiscordUserId != 0;

    public bool TryRemove()
    {
        lock( _lock)
        {
            if(DiscordUserId == 0)
            {
                DiscordUserId = 0;
                Name = null;
                Removed?.Invoke(this);
                return true;
            }
            return false;
        }
    }
}
