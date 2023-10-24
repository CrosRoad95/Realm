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
            .Where(x => x.Serial == serial && x.End > now);

        if (type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToListAsync(cancellationToken);
    }
    
    public async Task<List<BanData>> GetBansByUserId(int userId, DateTime now, int? type = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.UserId == userId && x.End > now);

        if(type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<BanData>> GetBansByUserIdOrSerial(int userId, string serial, DateTime now, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial || x.UserId == userId && x.End > now);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans.Where(x => x.Id == id)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        return await _db.Bans.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteByUserId(int userId, int type = 0, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans.Where(x => x.UserId == userId && x.Type == type)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));

        return await query.ExecuteDeleteAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteBySerial(string serial, int type = 0, CancellationToken cancellationToken = default)
    {
        var query = _db.Bans.Where(x => x.Serial == serial && x.Type == type)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));

        return await query.ExecuteDeleteAsync(cancellationToken) > 0;
    }
}
