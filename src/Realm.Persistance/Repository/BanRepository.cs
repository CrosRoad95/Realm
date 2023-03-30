using Realm.Common.Providers;

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
    
    public Task<List<Ban>> GetBansBySerial(string serial, IDateTimeProvider dateTimeProvider)
    {
        var query = _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.End > dateTimeProvider.Now);
        return query.ToListAsync();
    }
    
    public Task<Ban?> GetBanBySerialAndBanType(string serial, int banType, IDateTimeProvider dateTimeProvider)
    {
        var query = _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.Serial == serial && x.Type == banType && x.End > dateTimeProvider.Now);
        return query.FirstOrDefaultAsync();
    }
    
    public async Task<List<Ban>> GetBansByUserId(int userId, IDateTimeProvider dateTimeProvider)
    {
        return await _db.Bans
            .TagWithSource(nameof(BanRepository))
            .Where(x => x.UserId == userId && x.End > dateTimeProvider.Now)
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
