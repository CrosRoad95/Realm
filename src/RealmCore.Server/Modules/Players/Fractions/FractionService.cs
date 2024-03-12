namespace RealmCore.Server.Modules.Players.Fractions;

public interface IFractionService
{
    bool Exists(int fractionId);
    bool HasMember(int fractionId, int userId);
    Task LoadFractions(CancellationToken cancellationToken = default);
    Task AddMember(int fractionId, int userId, int rank, string rankName, CancellationToken cancellationToken = default);
    Task AddMember(int fractionId, RealmPlayer player, int rank, string rankName, CancellationToken cancellationToken = default);
    internal Task<bool> TryCreateFraction(int id, string fractionName, string fractionCode, Vector3 position, CancellationToken cancellationToken = default);
}

internal sealed class FractionService : IFractionService
{
    private readonly Dictionary<int, FractionDto> _fractions = [];
    private readonly object _lock = new();
    private readonly IFractionRepository _fractionRepository;
    private readonly IUsersInUse _usersInUse;

    public event Action<IFractionService, FractionDto>? Created;

    public FractionService(IFractionRepository fractionRepository, IUsersInUse usersInUse)
    {
        _fractionRepository = fractionRepository;
        _usersInUse = usersInUse;
    }

    public async Task LoadFractions(CancellationToken cancellationToken = default)
    {
        var fractionDataList = await _fractionRepository.GetAll(cancellationToken);
        lock (_lock)
        {
            foreach (var fractionData in fractionDataList)
            {
                var fractionDto = FractionDto.Map(fractionData);
                _fractions[fractionData.Id] = fractionDto;
                Created?.Invoke(this, fractionDto);
            }
        }
    }

    public bool Exists(int fractionId)
    {
        lock (_lock)
            return _fractions.ContainsKey(fractionId);
    }

    private bool InternalHasMember(int fractionId, int userId)
    {
        if (!_fractions.ContainsKey(fractionId))
            return false;

        return _fractions[fractionId].Members.Any(x => x.FractionId == userId);
    }

    public bool HasMember(int fractionId, int userId)
    {
        lock (_lock)
            return InternalHasMember(fractionId, userId);
    }

    public async Task AddMember(int fractionId, int userId, int rank, string rankName, CancellationToken cancellationToken = default)
    {
        if (_usersInUse.TryGetPlayerByUserId(userId, out var player) && player != null)
        {
            await AddMember(fractionId, player, rank, rankName, cancellationToken);
            return;
        }
        var memberData = await _fractionRepository.TryAddMember(fractionId, userId, rank, rankName, cancellationToken);
        if (memberData == null)
            throw new FractionMemberAlreadyAddedException(userId, fractionId);

        lock (_lock)
            _fractions[fractionId].Members.Add(FractionMemberDto.Map(memberData));
    }

    public async Task AddMember(int fractionId, RealmPlayer player, int rank, string rankName, CancellationToken cancellationToken = default)
    {
        var memberData = await _fractionRepository.TryAddMember(fractionId, player.PersistentId, rank, rankName, cancellationToken);
        if (memberData == null)
            throw new FractionMemberAlreadyAddedException(player.PersistentId, fractionId);
        player.Fractions.AddMember(memberData);

        lock (_lock)
            _fractions[fractionId].Members.Add(FractionMemberDto.Map(memberData));
    }

    public async Task<bool> TryCreateFraction(int fractionId, string fractionName, string fractionCode, Vector3 position, CancellationToken cancellationToken = default)
    {
        var fractionData = await _fractionRepository.TryCreate(fractionId, fractionName, fractionCode, cancellationToken);
        if (fractionData == null)
            return false;

        lock (_lock)
        {
            var fractionDto = FractionDto.Map(fractionData);
            _fractions[fractionData.Id] = fractionDto;
            Created?.Invoke(this, fractionDto);
        }

        return true;
    }
}
