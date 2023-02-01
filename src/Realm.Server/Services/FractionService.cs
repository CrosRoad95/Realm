using Realm.Domain.Concepts;
using System.Collections.Concurrent;
using Fraction = Realm.Domain.Concepts.Fraction;

namespace Realm.Server.Services;

internal class FractionService : IFractionService
{
    private readonly ConcurrentDictionary<int, Fraction> _fractions = new();

    public FractionService()
    {

    }

    public void CreateFraction(int id, string fractionName, string fractionCode, Vector3 position)
    {
        _fractions.TryAdd(id, new Fraction
        {
            id = id,
            name = fractionName,
            code = fractionCode,
            position = position
        });
    }
}
