namespace RealmCore.Server.Modules.Players.Avatars;

public sealed class PlayerAvatarFeature : IPlayerFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private UserData? _userData;
    private string? _current;

    public event Action<PlayerAvatarFeature, string?>? Changed;
    public event Action? VersionIncreased;

    public string? Current
    {
        set
        {
            lock (_lock)
            {
                if (_current == value)
                    return;

                if (_userData != null)
                    _userData.Avatar = value;
                _current = value;
            }

            Changed?.Invoke(this, value);
            VersionIncreased?.Invoke();
        }
        get
        {
            lock (_lock){
                return _userData?.Avatar;
            }
        }
    }

    public RealmPlayer Player { get; init; }
    public PlayerAvatarFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        lock (_lock)
        {
            _userData = userData;
            _current = _userData.Avatar;
        }
    }

    public void Reset()
    {
        Current = null;
    }
}
