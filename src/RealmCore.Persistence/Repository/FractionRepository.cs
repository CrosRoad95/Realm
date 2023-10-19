namespace RealmCore.Persistence.Repository;

internal sealed class FractionRepository : IFractionRepository
{
    private readonly IDb _db;

    public FractionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<List<FractionMemberData>> GetAllMembers(int fractionId, CancellationToken cancellationToken = default)
    {
        var query = _db.FractionMembers.Where(x => x.FractionId == fractionId)
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> Exists(int id, string code, string name, CancellationToken cancellationToken = default)
    {
        var query = _db.Fractions
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking()
            .Where(x => x.Id == id && x.Code == code && x.Name == name);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> CreateFraction(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default)
    {
        _db.Fractions.Add(new FractionData
        {
            Id = id,
            Name = fractionName,
            Code = fractionCode
        });

        return await _db.SaveChangesAsync(cancellationToken) == 1;
    }

    public async Task<bool> AddMember(int fractionId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default)
    {
        _db.FractionMembers.Add(new FractionMemberData
        {
            FractionId = fractionId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        });

        return await _db.SaveChangesAsync(cancellationToken) == 1;
    }
}
