namespace RealmCore.Persistence.Repository;

public interface IFractionRepository
{
    Task<bool> CreateFraction(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default);
    Task<bool> Exists(int id, string code, string name, CancellationToken cancellationToken = default);
    Task<List<FractionData>> GetAll(CancellationToken cancellationToken = default);
    Task<List<FractionMemberData>> GetAllMembers(int fractionId, CancellationToken cancellationToken = default);
    Task<FractionMemberData?> TryAddMember(int fractionId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default);
    Task<FractionData?> TryCreateFraction(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default);
}

internal sealed class FractionRepository : IFractionRepository
{
    private readonly IDb _db;
    private readonly ITransactionContext _transactionContext;

    public FractionRepository(IDb db, ITransactionContext transactionContext)
    {
        _db = db;
        _transactionContext = transactionContext;
    }

    public async Task<List<FractionData>> GetAll(CancellationToken cancellationToken = default)
    {
        var query = _db.Fractions
            .Include(x => x.Members)
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<FractionMemberData>> GetAllMembers(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.FractionMembers.Where(x => x.FractionId == id)
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

    public async Task<bool> CreateFraction(int id, string name, string code, CancellationToken cancellationToken = default)
    {
        _db.Fractions.Add(new FractionData
        {
            Id = id,
            Name = name,
            Code = code
        });

        return await _db.SaveChangesAsync(cancellationToken) == 1;
    }
    
    public async Task<FractionData?> TryCreateFraction(int id, string name, string code, CancellationToken cancellationToken = default)
    {
        var result = await _transactionContext.ExecuteAsync(async (db) =>
        {
            var existsQuery = _db.Fractions
                .TagWithSource(nameof(FractionRepository))
                .AsNoTracking()
                .Where(x => x.Id == id && x.Code == code && x.Name == name);
            if (await existsQuery.AnyAsync(cancellationToken))
                return null;

            var fractionData = new FractionData
            {
                Id = id,
                Name = name,
                Code = code
            };

            _db.Fractions.Add(fractionData);
            await _db.SaveChangesAsync(cancellationToken);
            return fractionData;
        }, cancellationToken);

        return result;
    }

    public async Task<FractionMemberData?> TryAddMember(int fractionId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default)
    {
        var fractionMember = new FractionMemberData
        {
            FractionId = fractionId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        };

        _db.FractionMembers.Add(fractionMember);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return fractionMember;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
