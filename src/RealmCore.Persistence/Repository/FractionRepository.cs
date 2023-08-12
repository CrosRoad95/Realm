namespace RealmCore.Persistence.Repository;

internal class FractionRepository : IFractionRepository
{
    private readonly IDb _db;

    public FractionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<List<FractionMemberData>> GetAllMembers(int fractionId) => await _db.FractionMembers.Where(x => x.FractionId == fractionId).ToListAsync();

    public Task<bool> Exists(int id, string code, string name)
    {
        var query = _db.Fractions
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking()
            .Where(x => x.Id == id && x.Code == code && x.Name == name);

        return query.AnyAsync();
    }

    public void CreateFraction(int id, string fractionName, string fractionCode)
    {
        _db.Fractions.Add(new FractionData
        {
            Id = id,
            Name = fractionName,
            Code = fractionCode
        });
    }

    public void AddFractionMember(int fractionId, int userId, int rank = 1, string rankName = "")
    {
        var fractionMember = new FractionMemberData
        {
            FractionId = fractionId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        };
        _db.FractionMembers.Add(fractionMember);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public Task<int> Commit()
    {
        return _db.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Commit();
        Dispose();
    }
}
