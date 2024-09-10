namespace RealmCore.Persistence.Repository;

public sealed class BanRepository
{
    private readonly IDb _db;

    public BanRepository(IDb db)
    {
        _db = db;
    }

    #region Write
    public async Task<UserBanData> CreateForSerial(string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateForSerial));
        if(activity != null)
        {
            activity.AddTag("Serial", serial);
            activity.AddTag("Until", until);
            activity.AddTag("Reason", reason);
            activity.AddTag("Responsible", responsible);
            activity.AddTag("Type", type);
        }

        if (reason != null && reason.Length > 255)
            throw new ArgumentOutOfRangeException(nameof(reason));

        var ban = new UserBanData
        {
            Serial = serial,
            End = until ?? DateTime.MaxValue,
            Reason = reason,
            Responsible = responsible,
            Type = type,
            Active = true
        };
        _db.Bans.Add(ban);
        await _db.SaveChangesAsync(cancellationToken);
        return ban;
    }

    public async Task<UserBanData> CreateForUserIdAndSerial(int userId, string serial, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateForUserIdAndSerial));
        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Serial", serial);
            activity.AddTag("Until", until);
            activity.AddTag("Reason", reason);
            activity.AddTag("Responsible", responsible);
            activity.AddTag("Type", type);
        }

        if (reason != null && reason.Length > 255)
            throw new ArgumentOutOfRangeException(nameof(reason));

        var ban = new UserBanData
        {
            UserId = userId,
            Serial = serial,
            End = until ?? DateTime.MaxValue,
            Reason = reason,
            Responsible = responsible,
            Type = type,
            Active = true
        };
        _db.Bans.Add(ban);
        await _db.SaveChangesAsync(cancellationToken);
        return ban;
    }

    public async Task<UserBanData> CreateForUserId(int userId, DateTime? until = null, string? reason = null, string? responsible = null, int type = 0, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateForUserId));
        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Until", until);
            activity.AddTag("Reason", reason);
            activity.AddTag("Responsible", responsible);
            activity.AddTag("Type", type);
        }

        if (reason != null && reason.Length > 255)
            throw new ArgumentOutOfRangeException(nameof(reason));

        var ban = new UserBanData
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
    }


    public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Delete));
        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.Bans.Where(x => x.Id == id && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));

        var banData = await query.FirstOrDefaultAsync(cancellationToken);
        if (banData == null)
            return false;

        if (!banData.Active)
            return false;
        banData.Active = false;
        _db.Bans.Update(banData);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int[]> DeleteByUserId(int userId, int? type = 0, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(DeleteByUserId));
        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Type", type);
        }

        var query = _db.Bans.Where(x => x.UserId == userId && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        if (type != null)
            query = query.Where(x => x.Type == type);

        var bansData = await query.ToListAsync(cancellationToken);
        if (bansData == null || bansData.Count == 0)
            return [];

        List<int> deletedBansIds = [];
        foreach (var banData in bansData)
        {
            if (!banData.Active)
                continue;
            banData.Active = true;
            _db.Bans.Update(banData);
            deletedBansIds.Add(banData.Id);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return [.. deletedBansIds];
    }

    public async Task<int[]> DeleteBySerial(string serial, int? type = 0, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(DeleteBySerial));
        if (activity != null)
        {
            activity.AddTag("Serial", serial);
            activity.AddTag("Type", type);
        }

        var query = _db.Bans.Where(x => x.Serial == serial && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        if (type != null)
            query = query.Where(x => x.Type == type);

        var bansData = await query.ToListAsync(cancellationToken);
        if (bansData == null || bansData.Count == 0)
            return [];

        List<int> deletedBansIds = [];
        foreach (var banData in bansData)
        {
            if (!banData.Active)
                continue;
            banData.Active = false;
            _db.Bans.Update(banData);
            deletedBansIds.Add(banData.Id);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return [.. deletedBansIds];
    }

    public async Task<int[]> DeleteByUserIdOrSerial(int userId, string serial, int? type = 0, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(DeleteByUserIdOrSerial));
        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Serial", serial);
            activity.AddTag("Type", type);
        }

        var query = _db.Bans.Where(x => (x.UserId == userId || x.Serial == serial) && x.Active)
            .AsNoTracking()
            .TagWithSource(nameof(BanRepository));
        if (type != null)
            query = query.Where(x => x.Type == type);

        var bansData = await query.ToListAsync(cancellationToken);
        if (bansData == null || bansData.Count == 0)
            return [];

        List<int> deletedBansIds = [];
        foreach (var banData in bansData)
        {
            if (!banData.Active)
                continue;
            banData.Active = true;
            _db.Bans.Update(banData);
            deletedBansIds.Add(banData.Id);
        }
        await _db.SaveChangesAsync(cancellationToken);
        return [.. deletedBansIds];
    }
    #endregion

    #region Read
    public async Task<UserBanData[]> GetBySerial(string serial, DateTime now, int? type = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetBySerial));
        if (activity != null)
        {
            activity.AddTag("Serial", serial);
            activity.AddTag("Type", type);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Serial == serial && x.End > now && x.Active);

        if (type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<UserBanData[]> GetByUserId(int userId, DateTime now, int? type = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByUserId));
        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Type", type);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.End > now && x.Active);

        if(type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<UserBanData[]> GetByUserIdOrSerial(int userId, string serial, DateTime now, int? type = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByUserIdOrSerial));
        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Serial", serial);
            activity.AddTag("Type", type);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => (x.Serial == serial || x.UserId == userId) && x.End > now && x.Active);

        if (type != null)
            query = query.Where(x => x.Type == type);

        return await query.ToArrayAsync(cancellationToken);
    }

    private IQueryable<UserBanData> CreateQueryBase() => _db.Bans.TagWithSource(nameof(BanRepository));
    #endregion

    public static readonly ActivitySource Activity = new("RealmCore.BanRepository", "1.0.0");
}
