namespace RealmCore.Persistence.Repository;

internal sealed class BanRepository : IBanRepository
{
    private readonly IDb _db;

    public BanRepository(IDb db)
    {
        _db = db;
    }

    public async Task<BanData> CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        var ban = new BanData
        {
            Serial = serial,
            End = until ?? DateTime.MaxValue,
            Reason = reason,
            Responsible = responsible,
            Type = type,
        };
        _db.Bans.Add(ban);
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ban;
    }

    public async Task<BanData> CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
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
        await _db.SaveChangesAsync().ConfigureAwait(false);
        return ban;
    }

    public async Task<List<BanData>> GetBansBySerial(string serial, DateTime now)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.End > now);

        return await query.ToListAsync().ConfigureAwait(false);
    }
    
    public async Task<BanData?> GetBanBySerialAndType(string serial, int type, DateTime now)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.Type == type && x.End > now);

        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<List<BanData>> GetBansByUserId(int userId, DateTime now)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.UserId == userId && x.End > now);

        return await query.ToListAsync().ConfigureAwait(false);
    }

    public async Task<List<BanData>> GetBansByUserIdOrSerial(int userId, string serial, DateTime now)
    {
        var query = _db.Bans
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial || x.UserId == userId && x.End > now);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public async Task<bool> Delete(int id)
    {
        var query = _db.Bans.Where(x => x.Id == id)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        return await _db.Bans.Where(x => x.Id == id).ExecuteDeleteAsync().ConfigureAwait(false) == 1;
    }

    public async Task<bool> DeleteByUserId(int userId, int type = 0)
    {
        var query = _db.Bans.Where(x => x.UserId == userId && x.Type == type)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));

        return await query.ExecuteDeleteAsync().ConfigureAwait(false) > 0;
    }

    public async Task<bool> DeleteBySerial(string serial, int type = 0)
    {
        var query = _db.Bans.Where(x => x.Serial == serial && x.Type == type)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));

        return await query.ExecuteDeleteAsync().ConfigureAwait(false) > 0;
    }
}
