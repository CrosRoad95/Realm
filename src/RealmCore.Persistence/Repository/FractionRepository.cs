namespace RealmCore.Persistence.Repository;

public interface IFractionRepository
{
    Task<bool> Create(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default);
    Task<bool> Exists(int id, string code, string name, CancellationToken cancellationToken = default);
    Task<FractionData[]> GetAll(CancellationToken cancellationToken = default);
    Task<FractionMemberData[]> GetAllMembers(int fractionId, CancellationToken cancellationToken = default);
    Task<FractionMemberData?> TryAddMember(int fractionId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default);
    Task<FractionData?> CreateOrGet(int id, string fractionName, string fractionCode, CancellationToken cancellationToken = default);
}

internal sealed class FractionRepository : IFractionRepository
{
    private readonly IDb _db;

    public FractionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<FractionData[]> GetAll(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAll));

        var query = _db.Fractions
            .Include(x => x.Members)
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking();

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<FractionMemberData[]> GetAllMembers(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllMembers));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.FractionMembers.Where(x => x.FractionId == id)
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking();

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> Exists(int id, string code, string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Exists));

        if (activity != null)
        {
            activity.AddTag("Id", id);
            activity.AddTag("Code", code);
            activity.AddTag("Name", name);
        }

        var query = _db.Fractions
            .TagWithSource(nameof(FractionRepository))
            .AsNoTracking()
            .Where(x => x.Id == id && x.Code == code && x.Name == name);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> Create(int id, string name, string code, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Create));

        if (activity != null)
        {
            activity.AddTag("Id", id);
            activity.AddTag("Code", code);
            activity.AddTag("Name", name);
        }

        _db.Fractions.Add(new FractionData
        {
            Id = id,
            Name = name,
            Code = code
        });

        return await _db.SaveChangesAsync(cancellationToken) == 1;
    }
    
    public async Task<FractionData> CreateOrGet(int id, string name, string code, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateOrGet));

        if (activity != null)
        {
            activity.AddTag("Id", id);
            activity.AddTag("Code", code);
            activity.AddTag("Name", name);
        }

        var query = _db.Fractions
            .TagWithSource(nameof(FractionRepository))
            .Include(x => x.Members)
            .AsNoTracking()
            .Where(x => x.Id == id && x.Code == code && x.Name == name);
        var existingFraction = await query.FirstOrDefaultAsync(cancellationToken);
        if (existingFraction != null)
            return existingFraction;

        var fractionData = new FractionData
        {
            Id = id,
            Name = name,
            Code = code
        };

        _db.Fractions.Add(fractionData);
        await _db.SaveChangesAsync(cancellationToken);
        return fractionData;
    }

    public async Task<FractionMemberData?> TryAddMember(int id, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryAddMember));

        if (activity != null)
        {
            activity.AddTag("Id", id);
            activity.AddTag("UserId", userId);
            activity.AddTag("Rank", rank);
            activity.AddTag("RankName", rankName);
        }

        var fractionMember = new FractionMemberData
        {
            FractionId = id,
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

    public static readonly ActivitySource Activity = new("RealmCore.FractionRepository", "1.0.0");
}
