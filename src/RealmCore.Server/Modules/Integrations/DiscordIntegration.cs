namespace RealmCore.Server.Modules.Integrations;

public interface IDiscordIntegration : IIntegration
{
    ulong DiscordUserId { get; }
    string? Name { get; }

    string GenerateAndGetConnectionCode(TimeSpan? validFor = null);
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
                throw new IntegrationAlreadyCreatedException();

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
                throw new IntegrationAlreadyCreatedException();

            if (!HasPendingDiscordConnectionCode())
                return false;

            if (_discordConnectionCode == code)
            {
                Integrate(discordUserId, name);
                return true;
            }
        }

        return false;
    }

    public string GenerateAndGetConnectionCode(TimeSpan? validFor = null)
    {
        lock (_lock)
        {
            if (IsIntegrated())
                throw new IntegrationAlreadyCreatedException();

            _discordConnectionCode = Guid.NewGuid().ToString();
            _discordConnectionCodeValidUntil = _dateTimeProvider.Now.Add(validFor ?? TimeSpan.FromMinutes(2));
            return _discordConnectionCode;
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
                Removed?.Invoke(this);
                return true;
            }
            return false;
        }
    }
}
