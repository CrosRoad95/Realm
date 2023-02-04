using Realm.Domain.Exceptions;
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

    public void CreateFraction(int id, string fractionName, string fractionCode, Vector3 position)
    {
        lock(_fractionLock)
            _fractions.Add(id, new Fraction
            {
                id = id,
                name = fractionName,
                code = fractionCode,
                position = position,
                members = new()
            });
    }

    public void InternalAddMember(int fractionId, int userId, int rank, string rankName)
    {
        lock (_fractionLock)
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
    }

    public async Task AddMember(int fractionId, int userId, int rank, string rankName)
    {
        InternalAddMember(fractionId, userId, rank, rankName);

        await _fractionRepository.CreateNewFractionMember(fractionId, userId, rank, rankName);
    }
}
