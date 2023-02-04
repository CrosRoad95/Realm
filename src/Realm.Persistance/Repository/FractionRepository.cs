namespace Realm.Persistance.Repository;

internal class FractionRepository : IFractionRepository
{
    private readonly IDb _db;

    public FractionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<FractionMember> CreateNewFractionMember(int fractionId, int userId, int rank = 1, string rankName = "")
    {
        var fractionMember = new FractionMember
        {
            FractionId = fractionId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        };
        _db.FractionMembers.Add(fractionMember);
        await _db.SaveChangesAsync();
        return fractionMember;
    }
}
