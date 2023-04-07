namespace RealmCore.Persistance.Repository;

internal class BanRepository : IBanRepository
{
    private readonly IDb _db;

    public BanRepository(IDb db)
    {
        _db = db;
    }

    public BanData CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
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
        return ban;
    }

    public BanData CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
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
        return ban;
    }

    public Task<List<BanData>> GetBansBySerial(string serial, DateTime now)
    {
        var query = _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.End > now);
        return query.ToListAsync();
    }

    public Task<BanData?> GetBanBySerialAndBanType(string serial, int banType, DateTime now)
    {
        var query = _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.Type == banType && x.End > now);
        return query.FirstOrDefaultAsync();
    }

    public async Task<List<BanData>> GetBansByUserId(int userId, DateTime now)
    {
        return await _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.UserId == userId && x.End > now)
            .ToListAsync();
    }

    public void RemoveBan(BanData ban)
    {
        _db.Bans.Remove(ban);
    }

    public Task Commit()
    {
        return _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
