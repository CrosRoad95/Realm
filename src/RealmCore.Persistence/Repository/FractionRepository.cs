namespace RealmCore.Persistence.Repository;

internal sealed class FractionRepository : IFractionRepository
{
    private readonly IDb _db;

    public FractionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<List<FractionMemberData>> GetAllMembers(int fractionId)
    {
        var query = _db.FractionMembers.Where(x => x.FractionId == fractionId)
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking();

        return await query.ToListAsync().ConfigureAwait(false);
    }

    public async Task<bool> Exists(int id, string code, string name)
    {
        var query = _db.Fractions
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking()
            .Where(x => x.Id == id && x.Code == code && x.Name == name);

        return await query.AnyAsync().ConfigureAwait(false);
    }

    public async Task<bool> CreateFraction(int id, string fractionName, string fractionCode)
    {
        _db.Fractions.Add(new FractionData
        {
            Id = id,
            Name = fractionName,
            Code = fractionCode
        });

        return await _db.SaveChangesAsync().ConfigureAwait(false) == 1;
    }

    public async Task<bool> AddMember(int fractionId, int userId, int rank = 1, string rankName = "")
    {
        _db.FractionMembers.Add(new FractionMemberData
        {
            FractionId = fractionId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        });

        return await _db.SaveChangesAsync().ConfigureAwait(false) == 1;
    }
}
