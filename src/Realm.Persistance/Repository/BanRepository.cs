namespace Realm.Persistance.Repository;

internal class BanRepository : IBanRepository
{
    private readonly IDb _db;

    public BanRepository(IDb db)
    {
        _db = db;
    }

    public Ban CreateBanForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        var ban = new Ban
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
    
    public Ban CreateBanForUser(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0)
    {
        var ban = new Ban
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
    
    public Task<List<Ban>> GetBansBySerial(string serial)
    {
        var query = _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.End > DateTime.Now);
        return query.ToListAsync();
    }
    
    public async Task<List<Ban>> GetBansByUserId(int userId)
    {
        return await _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.UserId == userId && x.End > DateTime.Now)
            .ToListAsync();
    }

    public void RemoveBan(Ban ban)
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
