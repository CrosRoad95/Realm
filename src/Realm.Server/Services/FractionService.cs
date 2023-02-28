using Realm.Domain.Exceptions;
using Realm.Persistance.Data;
using Realm.Persistance.Interfaces;
using Fraction = Realm.Domain.Concepts.Fraction;
using FractionMember = Realm.Domain.Concepts.FractionMember;

namespace Realm.Server.Services;

internal class FractionService : IFractionService
{
    private readonly Dictionary<int, Fraction> _fractions = new();
    private readonly object _fractionLock = new object();
    private readonly IFractionRepository _fractionRepository;

    public FractionService(IFractionRepository fractionRepository)
    {
        _fractionRepository = fractionRepository;
    }

    public Task<bool> InternalExists(int id, string code, string name) => _fractionRepository.Exists(id, code, name);

    public bool Exists(int id)
    {
        lock (_fractionLock)
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
        lock (_fractionLock)
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

    public async Task InternalCreateFraction(int fractionId, string fractionName, string fractionCode, Vector3 position)
    {
        var members = await _fractionRepository.GetAllMembers(fractionId);
        lock (_fractionLock)
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

        if(!await _fractionRepository.Exists(fractionId, fractionCode, fractionName))
            _fractionRepository.CreateFraction(fractionId, fractionName, fractionCode);
            await _fractionRepository.Commit();
    }

    public async Task<bool> TryAddMember(int fractionId, int userId, int rank, string rankName)
    {
        lock (_fractionLock)
        {
            if (InternalHasMember(fractionId, userId))
                return false;
            InternalAddMember(fractionId, userId, rank, rankName);
        }
        
        _fractionRepository.AddFractionMember(fractionId, userId, rank, rankName);
        await _fractionRepository.Commit();
        return true;
    }
}
