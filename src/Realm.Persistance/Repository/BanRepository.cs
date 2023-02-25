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
    
    public async Task<List<Ban>> GetBansBySerial(string serial)
    {
        return await _db.Bans
            .Where(x => x.Serial == serial && x.End > DateTime.Now)
            .ToListAsync();
    }
    
    public async Task<List<Ban>> GetBansByUserId(int userId)
    {
        return await _db.Bans
            .Where(x => x.UserId == userId && x.End > DateTime.Now)
            .ToListAsync();
    }

    public void RemoveBan(Ban ban)
    {
        _db.Bans.Remove(ban);
    }

    public async Task Commit()
    {
        await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
