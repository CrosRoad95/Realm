namespace RealmCore.Persistence.Repository;

public sealed class MapsRepository
{
    private readonly IDb _db;

    public MapsRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> CreatePersistentMapFromFileUploa(int fileUploadId, int locationId, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreatePersistentMapFromFileUploa));

        if (activity != null)
        {
            activity.AddTag("FileUploadId", fileUploadId);
            activity.AddTag("LocationId", locationId);
        }

        var map = new MapData
        {
            FileUploadId = fileUploadId,
            LocationId = locationId,
            Metadata = metadata,
            Loaded = false
        };

        try
        {
            _db.Maps.Add(map);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return true;
    }

    public async Task<bool> CreatePersistentMapFromFileUploadForUser(int fileUploadId, int userId, int locationId, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreatePersistentMapFromFileUploadForUser));

        if (activity != null)
        {
            activity.AddTag("FileUploadId", fileUploadId);
            activity.AddTag("UserId", userId);
            activity.AddTag("LocationId", locationId);
        }

        var map = new MapData
        {
            FileUploadId = fileUploadId,
            LocationId = locationId,
            Metadata = metadata,
            Loaded = false,
            MapsUsers = [new MapUserData
            {
                UserId = userId
            }]
        };

        try
        {
            _db.Maps.Add(map);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return true;
    }

    public async Task<bool> SetMapLoaded(int mapId, bool loaded, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetMapLoaded));

        if (activity != null)
        {
            activity.AddTag("MapId", mapId);
            activity.AddTag("Loaded", loaded);
        }

        var query = _db.Maps
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == mapId && x.Loaded != loaded);

        try
        {
            return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Loaded, loaded), cancellationToken) == 1;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<MapData[]> GetMapsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetMapsByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.Maps
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.MapsUsers.Where(y => y.UserId == userId).Any());

        try
        {
            return await query.ToArrayAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public static readonly ActivitySource Activity = new("RealmCore.MapsRepository", "1.0.0");
}
