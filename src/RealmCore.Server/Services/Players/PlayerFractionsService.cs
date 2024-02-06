namespace RealmCore.Server.Services.Players;

public interface IPlayerFractionsService : IPlayerService
{
    internal bool AddFractionMember(FractionMemberData groupMemberData);
    internal bool RemoveGroupMember(int fractionId);
    bool IsMember(int fractionId);
    FractionMemberData? GetMemberOrDefault(int fractionId);
}

internal sealed class PlayerFractionsService : IPlayerFractionsService
{
    private readonly SemaphoreSlim _lock = new(1);
    private readonly IPlayerUserService _playerUserService;
    private ICollection<FractionMemberData> _fractionMembers = [];
    public RealmPlayer Player { get; private set; }
    public PlayerFractionsService(PlayerContext playerContext, IPlayerUserService playerUserService)
    {
        Player = playerContext.Player;
        playerUserService.SignedIn += HandleSignedIn;
        playerUserService.SignedOut += HandleSignedOut;
        _playerUserService = playerUserService;
    }

    private void HandleSignedIn(IPlayerUserService playerUserService, RealmPlayer _)
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

    private void HandleSignedOut(IPlayerUserService playerUserService, RealmPlayer _)
    {

    }
}
