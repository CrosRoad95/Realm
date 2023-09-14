using Fraction = RealmCore.Server.Structs.Fraction;
using FractionMember = RealmCore.Server.Structs.FractionMember;

namespace RealmCore.Server.Services;

internal sealed class FractionService : IFractionService
{
    private readonly Dictionary<int, Fraction> _fractions = new();
    private readonly object _lock = new();
    private readonly IFractionRepository _fractionRepository;

    public FractionService(IFractionRepository fractionRepository)
    {
        _fractionRepository = fractionRepository;
    }

    public Task<bool> InternalExists(int id, string code, string name) => _fractionRepository.Exists(id, code, name);

    public bool Exists(int id)
    {
        lock (_lock)
            return _fractions.ContainsKey(id);
    }

    private bool InternalHasMember(int fractionId, int userId)
    {
        if (!_fractions.ContainsKey(fractionId))
            return false;

        return _fractions[fractionId].members.Any(x => x.userId == userId);
    }

    public bool HasMember(int fractionId, int userId)
    {
        lock (_lock)
            return InternalHasMember(fractionId, userId);
    }

    private void InternalAddMember(int fractionId, int userId, int rank, string rankName)
    {
        var fraction = _fractions[fractionId];
        if (fraction.members.Any(x => x.userId == userId))
            throw new FractionMemberAlreadyAddedException(userId, fractionId);
        fraction.members.Add(new FractionMember
        {
            rank = rank,
            userId = userId,
            rankName = rankName
        });
        _fractions[fractionId] = fraction;
    }

    public async Task<bool> InternalCreateFraction(int fractionId, string fractionName, string fractionCode, Vector3 position)
    {
        var members = await _fractionRepository.GetAllMembers(fractionId);
        lock (_lock)
            _fractions.Add(fractionId, new Fraction
            {
                id = fractionId,
                name = fractionName,
                code = fractionCode,
                position = position,
                members = members.Select(x => new FractionMember
                {
                    userId = x.UserId,
                    rank = x.Rank,
                    rankName = x.RankName,
                }).ToList()
            });

        if (!await _fractionRepository.Exists(fractionId, fractionCode, fractionName))
        {
            await _fractionRepository.CreateFraction(fractionId, fractionName, fractionCode);
            return true;
        }
        return false;
    }

    public async Task<bool> TryAddMember(int fractionId, int userId, int rank, string rankName)
    {
        lock (_lock)
        {
            if (InternalHasMember(fractionId, userId))
                return false;
            InternalAddMember(fractionId, userId, rank, rankName);
        }

        return await _fractionRepository.AddMember(fractionId, userId, rank, rankName).ConfigureAwait(false);
    }
}
