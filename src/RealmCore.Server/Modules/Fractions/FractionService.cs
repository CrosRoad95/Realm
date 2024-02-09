namespace RealmCore.Server.Modules.Fractions;

public interface IFractionService
{
    bool Exists(int id);
    bool HasMember(int fractionId, int userId);
    internal Task<bool> TryCreateFraction(int id, string fractionName, string fractionCode, Vector3 position, CancellationToken cancellationToken = default);
    internal Task<bool> InternalExists(int id, string code, string name, CancellationToken cancellationToken = default);
    Task LoadFractions(CancellationToken cancellationToken);
    Task<bool> TryAddMember(int fractionId, int userId, int rank, string rankName);
}

internal sealed class FractionService : IFractionService
{
    private readonly Dictionary<int, FractionData> _fractions = [];
    private readonly object _lock = new();
    private readonly IFractionRepository _fractionRepository;

    public FractionService(IFractionRepository fractionRepository)
    {
        _fractionRepository = fractionRepository;
    }

    public async Task LoadFractions(CancellationToken cancellationToken)
    {
        var fractions = await _fractionRepository.GetAll(cancellationToken);
        lock (_lock)
        {
            foreach (var fraction in fractions)
            {
                _fractions[fraction.Id] = fraction;
            }
        }
    }

    public Task<bool> InternalExists(int id, string code, string name, CancellationToken cancellationToken = default) => _fractionRepository.Exists(id, code, name, cancellationToken);

    public bool Exists(int id)
    {
        lock (_lock)
            return _fractions.ContainsKey(id);
    }

    private bool InternalHasMember(int fractionId, int userId)
    {
        if (!_fractions.ContainsKey(fractionId))
            return false;

        return _fractions[fractionId].Members.Any(x => x.UserId == userId);
    }

    public bool HasMember(int fractionId, int userId)
    {
        lock (_lock)
            return InternalHasMember(fractionId, userId);
    }

    public async Task<bool> TryAddMember(int fractionId, int userId, int rank, string rankName)
    {
        var memberData = await _fractionRepository.TryAddMember(fractionId, userId, rank, rankName);
        if (memberData == null)
            return false;

        lock (_lock)
            _fractions[fractionId].Members.Add(memberData);
        return true;
    }

    public async Task TryAddMember(int fractionId, RealmPlayer player, int rank, string rankName)
    {
        var memberData = await _fractionRepository.TryAddMember(fractionId, player.UserId, rank, rankName);
        if (memberData == null)
            throw new FractionMemberAlreadyAddedException(player.UserId, fractionId);
        player.Fractions.AddFractionMember(memberData);

        lock (_lock)
            _fractions[fractionId].Members.Add(memberData);
    }

    public async Task<bool> TryCreateFraction(int fractionId, string fractionName, string fractionCode, Vector3 position, CancellationToken cancellationToken = default)
    {
        var fraction = await _fractionRepository.TryCreateFraction(fractionId, fractionName, fractionCode, cancellationToken);
        return fraction != null;
    }
}
