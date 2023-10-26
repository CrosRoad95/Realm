namespace RealmCore.Persistence.Repository;

internal sealed class BanRepository : IBanRepository
{
    private readonly IDb _db;
    private readonly ITransactionContext _transactionContext;

    public BanRepository(IDb db, ITransactionContext transactionContext)
    {
        _db = db;
        _transactionContext = transactionContext;
    }

    public async Task<BanData> CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        if (reason != null && reason.Length > 255)
            throw new ArgumentOutOfRangeException(nameof(reason));

        var ban = await _transactionContext.ExecuteAsync(async db =>
        {
            var ban = new BanData
            {
                Serial = serial,
                End = until ?? DateTime.MaxValue,
                Reason = reason,
                Responsible = responsible,
                Type = type,
                Active = true
            };
            db.Bans.Add(ban);
            await db.SaveChangesAsync(cancellationToken);
            return ban;
        }, cancellationToken);
        return ban;
    }

    public async Task<BanData> CreateBanForUserIdAndSerial(int userId, string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        if (reason != null && reason.Length > 255)
            throw new ArgumentOutOfRangeException(nameof(reason));

        var ban = await _transactionContext.ExecuteAsync(async db =>
        {
            var ban = new BanData
            {
                UserId = userId,
                Serial = serial,
                End = until ?? DateTime.MaxValue,
                Reason = reason,
                Responsible = responsible,
                Type = type,
                Active = true
            };
            db.Bans.Add(ban);
            await db.SaveChangesAsync(cancellationToken);
            return ban;
        }, cancellationToken);
        return ban;
    }

    public async Task<BanData> CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        if (reason != null && reason.Length > 255)
            throw new ArgumentOutOfRangeException(nameof(reason));

        var ban = await _transactionContext.ExecuteAsync(async db =>
        {
            var ban = new BanData
            {
                UserId = userId,
                End = until ?? DateTime.MaxValue,
                Reason = reason,
                Responsible = responsible,
                Type = type,
                Active = true
            };
            _db.Bans.Add(ban);
            await _db.SaveChangesAsync(cancellationToken);
            return ban;
        }, cancellationToken);
        return ban;
    }

    public async Task<List<BanData>> GetBansBySerial(string serial, DateTime now, int? type = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.End > now && x.Active);

        if (type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToListAsync(cancellationToken);
    }
    
    public async Task<List<BanData>> GetBansByUserId(int userId, DateTime now, int? type = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.UserId == userId && x.End > now && x.Active);

        if(type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<BanData>> GetBansByUserIdOrSerial(int userId, string serial, DateTime now, int? type = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => (x.Serial == serial || x.UserId == userId) && x.End > now && x.Active);

        if (type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans.Where(x => x.Id == id && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));

        var banData = await query.FirstOrDefaultAsync();
        if (banData == null)
            return false;

        if (!banData.Active)
            return false;
        banData.Active = false;
        _db.Bans.Update(banData);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<int>> DeleteByUserId(int userId, int? type = 0, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans.Where(x => x.UserId == userId && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        if (type != null)
            query = query.Where(x => x.Type == type);

        var bansData = await query.ToListAsync();
        if (bansData == null || bansData.Count == 0)
            return new();

        List<int> deletedBansIds = new();
        foreach (var banData in bansData)
        {
            if (!banData.Active)
                continue;
            banData.Active = true;
            _db.Bans.Update(banData);
            deletedBansIds.Add(banData.Id);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return deletedBansIds;
    }

    public async Task<List<int>> DeleteBySerial(string serial, int? type = 0, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans.Where(x => x.Serial == serial && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        if (type != null)
            query = query.Where(x => x.Type == type);

        var bansData = await query.ToListAsync();
        if (bansData == null || bansData.Count == 0)
            return new();

        List<int> deletedBansIds = new();
        foreach (var banData in bansData)
        {
            if (!banData.Active)
                continue;
            banData.Active = false;
            _db.Bans.Update(banData);
            deletedBansIds.Add(banData.Id);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return deletedBansIds;
    }

    public async Task<List<int>> DeleteByUserIdOrSerial(int userId, string serial, int? type = 0, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans.Where(x => (x.UserId == userId || x.Serial == serial) && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        if (type != null)
            query = query.Where(x => x.Type == type);

        var bansData = await query.ToListAsync();
        if (bansData == null || bansData.Count == 0)
            return new();

        List<int> deletedBansIds = new();
        foreach (var banData in bansData)
        {
            if (!banData.Active)
                continue;
            banData.Active = true;
            _db.Bans.Update(banData);
            deletedBansIds.Add(banData.Id);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return deletedBansIds;
    }
}
