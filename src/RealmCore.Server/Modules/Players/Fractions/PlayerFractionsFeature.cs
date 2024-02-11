namespace RealmCore.Server.Modules.Players.Membership;

public interface IPlayerFractionsFeature : IPlayerFeature
{
    bool IsMember(int fractionId);
    FractionMemberDto GetById(int fractionId);

    internal void AddMember(FractionMemberData fractionMemberData);
    internal bool RemoveMember(int fractionId);
}

internal sealed class PlayerFractionsFeature : IPlayerFractionsFeature
{
    private readonly object _lock = new();
    private readonly IPlayerUserFeature _playerUserFeature;
    private ICollection<FractionMemberData> _fractionMembers = [];

    public event Action<IPlayerFractionsFeature, FractionMemberDto>? Added;
    public event Action<IPlayerFractionsFeature, FractionMemberDto>? Removed;

    public RealmPlayer Player { get; init; }
    public PlayerFractionsFeature(PlayerContext playerContext, IPlayerUserFeature playerUserFeature)
    {
        Player = playerContext.Player;
        playerUserFeature.SignedIn += HandleSignedIn;
        playerUserFeature.SignedOut += HandleSignedOut;
        _playerUserFeature = playerUserFeature;
    }

    private void HandleSignedIn(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock(_lock)
            _fractionMembers = playerUserFeature.User.FractionMembers;
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
                _playerUserFeature.IncreaseVersion();
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

        _playerUserFeature.IncreaseVersion();
        return true;
    }

    private void HandleSignedOut(IPlayerUserFeature playerUserFeature, RealmPlayer _)
    {
        lock (_lock)
            _fractionMembers = [];
    }
}
