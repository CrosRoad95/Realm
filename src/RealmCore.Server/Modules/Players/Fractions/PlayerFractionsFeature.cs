namespace RealmCore.Server.Modules.Players.Membership;

public interface IPlayerFractionsFeature : IPlayerFeature
{
    bool IsMember(int fractionId);
    FractionMemberDto GetById(int fractionId);

    internal void AddMember(FractionMemberData fractionMemberData);
    internal bool RemoveMember(int fractionId);
}

internal sealed class PlayerFractionsFeature : IPlayerFractionsFeature, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<FractionMemberData> _fractionMembers = [];

    public event Action<IPlayerFractionsFeature, FractionMemberDto>? Added;
    public event Action<IPlayerFractionsFeature, FractionMemberDto>? Removed;
    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }
    public PlayerFractionsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void SignIn(UserData userData)
    {
        lock (_lock)
            _fractionMembers = userData.FractionMembers;
    }

    public void SignOut()
    {
        lock (_lock)
            _fractionMembers = [];
    }

    public bool IsMember(int fractionId)
    {
        lock (_lock)
            return _fractionMembers.Any(x => x.FractionId == fractionId);
    }
    
    public FractionMemberDto GetById(int fractionId)
    {
        lock (_lock)
            return FractionMemberDto.Map(_fractionMembers.First(x => x.FractionId == fractionId));
    }

    public void AddMember(FractionMemberData fractionMemberData)
    {
        lock (_lock)
        {
            var fraction = _fractionMembers.FirstOrDefault(x => x.FractionId == fractionMemberData.FractionId);
            if(fraction != null)
            {
                _fractionMembers.Add(fractionMemberData);
                Added?.Invoke(this, FractionMemberDto.Map(fractionMemberData));
                VersionIncreased?.Invoke();
            }
        }
    }

    public bool RemoveMember(int fractionId)
    {
        lock (_lock)
        {
            var fraction = _fractionMembers.FirstOrDefault(x => x.FractionId == fractionId);
            if (fraction == null)
                return false;
            _fractionMembers.Remove(fraction);
            Removed?.Invoke(this, FractionMemberDto.Map(fraction));
        }

        VersionIncreased?.Invoke();
        return true;
    }
}
