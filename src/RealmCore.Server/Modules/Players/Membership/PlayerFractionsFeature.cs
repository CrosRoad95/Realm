namespace RealmCore.Server.Modules.Players.Membership;

public interface IPlayerFractionsFeature : IPlayerFeature
{
    internal bool AddFractionMember(FractionMemberData groupMemberData);
    internal bool RemoveGroupMember(int fractionId);
    bool IsMember(int fractionId);
    FractionMemberData? GetMemberOrDefault(int fractionId);
}

internal sealed class PlayerFractionsFeature : IPlayerFractionsFeature
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IPlayerUserFeature _playerUserService;
    private ICollection<FractionMemberData> _fractionMembers = [];
    public RealmPlayer Player { get; init; }
    public PlayerFractionsFeature(PlayerContext playerContext, IPlayerUserFeature playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserService, RealmPlayer _)
    {
        _lock.Wait();
        try
        {
            _fractionMembers = playerUserService.User.FractionMembers;
        }
        finally
        {
            _lock.Release();
        }
    }

    public bool AddFractionMember(FractionMemberData fractionMemberData)
    {
        _lock.Wait();
        try
        {
            var group = _fractionMembers.FirstOrDefault(x => x.FractionId == fractionMemberData.FractionId);
            if (group == null)
                return false;

            _fractionMembers.Add(fractionMemberData);
        }
        finally
        {
            _lock.Release();
        }
        _playerUserService.IncreaseVersion();
        return true;
    }

    public bool RemoveGroupMember(int groupId)
    {
        _lock.Wait();
        try
        {
            var group = _fractionMembers.FirstOrDefault(x => x.FractionId == groupId);
            if (group == null)
                return false;
            _fractionMembers.Remove(group);
        }
        finally
        {
            _lock.Release();
        }
        _playerUserService.IncreaseVersion();
        return true;
    }

    public bool IsMember(int fractionId)
    {
        _lock.Wait();
        try
        {
            return _fractionMembers.Any(x => x.FractionId == fractionId);
        }
        finally
        {
            _lock.Release();
        }
    }


    public FractionMemberData? GetMemberOrDefault(int fractionId)
    {
        _lock.Wait();
        try
        {
            return _fractionMembers.FirstOrDefault(x => x.FractionId == fractionId);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserService, RealmPlayer _)
    {

    }
}
