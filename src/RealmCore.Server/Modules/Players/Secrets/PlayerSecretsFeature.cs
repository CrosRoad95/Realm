﻿namespace RealmCore.Server.Modules.Players.Secrets;

public sealed class PlayerSecretsFeature : IPlayerFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action? VersionIncreased;
    private ICollection<UserSecretsData> _secrets = [];

    public RealmPlayer Player { get; }

    public PlayerSecretsFeature(PlayerContext playerContext, IDateTimeProvider dateTimeProvider)
    {
        Player = playerContext.Player;
        _dateTimeProvider = dateTimeProvider;
    }

    public int[] GetByGroupId(int groupId)
    {
        lock (_lock)
        {
            return _secrets.Where(x => x.GroupId == groupId).Select(x => x.SecretId).ToArray();
        }
    }

    public bool TryReveal(int groupId, int secretId)
    {
        lock (_lock)
        {
            if (_secrets.Any(x => x.GroupId == groupId && x.SecretId == secretId))
                return false;

            _secrets.Add(new UserSecretsData
            {
                GroupId = groupId,
                SecretId = secretId,
                CreatedAt = _dateTimeProvider.Now
            });
        }

        VersionIncreased?.Invoke();

        return true;
    }

    public void LogIn(UserData userData)
    {
        lock (_secrets)
        {
            _secrets = userData.Secrets;
        }
    }
}
