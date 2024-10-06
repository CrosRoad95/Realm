namespace RealmCore.Persistence.Repository;

public sealed class VehiclesRepository
{
    private readonly IDb _db;

    public VehiclesRepository(IDb db)
    {
        _db = db;
    }

    public async Task<VehicleData> CreateVehicle(ushort model, DateTime now, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateVehicle));

        if (activity != null)
        {
            activity.AddTag("Model", model);
        }

        var vehicle = new VehicleData
        {
            Model = model,
            Platetext = Guid.NewGuid().ToString()[..8],
            CreatedAt = now,
            Color = new(),
            DamageState = new(),
            DoorOpenRatio = new(),
            TransformAndMotion = new(),
            Variant = new(),
            WheelStatus = new(),
            Spawned = true,
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync(cancellationToken);
        return vehicle;
    }

    public async Task<LightInfoVehicleDto[]> GetLightVehiclesByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLightVehiclesByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.UserAccesses.Any(x => x.UserId == userId))
            .Select(x => new LightInfoVehicleDto
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<LightInfoVehicleDto?> GetLightVehicleById(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLightVehicleById));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == vehicleId)
            .Select(x => new LightInfoVehicleDto
            {
                Id = x.Id,
                Model = x.Model,
                Position = x.TransformAndMotion.Position
            });

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<VehicleData[]> GetVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetVehiclesByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessTypes", accessTypes);
        }

        var query = BuildGetVehiclesByUserIdQuery(userId, accessTypes);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<int> CountVehiclesByUserId(int userId, IEnumerable<int>? accessTypes = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CountVehiclesByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessTypes", accessTypes);
        }

        var query = BuildGetVehiclesByUserIdQuery(userId, accessTypes);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int[]> GetAllSpawnedVehiclesIds(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllSpawnedVehiclesIds));

        var query = CreateQueryBase()
            .IsSpawned()
            .Select(x => x.Id);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<VehicleData?> GetById(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetById));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = CreateQueryBase()
            .Where(x => x.Id == vehicleId)
            .IncludeAll();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TrySetSpawned(int vehicleId, bool spawned, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TrySetSpawned));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("Spawned", spawned);
        }

        var query = CreateQueryBase()
            .Where(x => x.Id == vehicleId);

        var vehicle = await query.FirstAsync(cancellationToken);
        if (vehicle.Spawned == spawned)
            return false;

        vehicle.Spawned = spawned;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetKind(int vehicleId, byte kind, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetKind));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("Kind", kind);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == vehicleId);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Kind, kind), cancellationToken);
        return result > 0;
    }

    public async Task<bool> IsSpawned(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(IsSpawned));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == vehicleId)
            .Select(x => x.Spawned);

        return await query.FirstAsync(cancellationToken);
    }

    public async Task<bool> SoftRemove(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SoftRemove));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == vehicleId);

        var result = await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.IsRemoved, true), cancellationToken);
        return result > 0;
    }

    public async Task<VehicleGroupAccessData[]> GetGroupAccesses(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupAccesses));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = _db.VehicleGroupAccesses
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Include(x => x.Group)
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId);

        var groupAccesses = await query.ToArrayAsync(cancellationToken);

        return groupAccesses;
    }

    public async Task<VehicleUserAccessData[]> GetUserAccesses(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetUserAccesses));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId);

        var userAccesses = await query.ToArrayAsync(cancellationToken);

        return userAccesses;
    }

    public async Task<VehicleAccessDataBase[]> GetAllAccesses(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAllAccesses));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query1 = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId);
        
        var query2 = _db.VehicleGroupAccesses
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Include(x => x.Group)
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId);

        var userAccesses = await query1.ToArrayAsync(cancellationToken);
        var groupAccesses = await query2.ToArrayAsync(cancellationToken);

        return [.. userAccesses, .. groupAccesses];
    }

    public async Task<bool> HasUserAccessTo(int vehicleId, int userId, int[]? accessType = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(HasUserAccessTo));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.UserId == userId && (accessType == null || accessType.Contains(x.AccessType)))
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Select(x => x.UserId);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int[]> GetOwner(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetOwner));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.Vehicle != null && !x.Vehicle.IsRemoved)
            .Where(x => x.VehicleId == vehicleId && x.AccessType == 0)
            .Select(x => x.UserId);
        return await query.ToArrayAsync(cancellationToken);
    }

    private IQueryable<VehicleData> BuildGetVehiclesByUserIdQuery(int userId, IEnumerable<int>? accessTypes)
    {
        if (accessTypes != null && !accessTypes.Any())
        {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        var query = CreateQueryBase()
            .AsNoTracking();

        if (accessTypes != null)
        {
            return query.Where(x => x.UserAccesses.Any(y => y.UserId == userId && accessTypes.Contains(y.AccessType)));
        }
        else
        {
            return query.Where(x => x.UserAccesses.Any(y => y.UserId == userId));
        }
    }

    public async Task<bool> SetSetting(int vehicleId, int settingId, string? value, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetSetting));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("SettingId", settingId);
        }

        var query = _db.VehicleSettings
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.SettingId == settingId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Value, value), cancellationToken) == 1;
    }
    
    public async Task<bool> SetSetting(int[] vehiclesIds, int settingId, string? value, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetSetting));

        if (activity != null)
        {
            activity.AddTag("VehiclesIds", vehiclesIds);
            activity.AddTag("SettingId", settingId);
        }

        var query = _db.VehicleSettings
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => vehiclesIds.Contains(x.VehicleId) && x.SettingId == settingId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Value, value), cancellationToken) > 0;
    }
    
    public async Task<bool> RemoveAllSettings(int vehicleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveAllSettings));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
        }

        var query = _db.VehicleSettings
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId);

        return await query.ExecuteDeleteAsync(cancellationToken) > 0;
    }
    
    public async Task<bool> RemoveAllSettings(int[] vehiclesIds, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveAllSettings));

        if (activity != null)
        {
            activity.AddTag("VehiclesIds", vehiclesIds);
        }

        var query = _db.VehicleSettings
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => vehiclesIds.Contains(x.VehicleId));

        return await query.ExecuteDeleteAsync(cancellationToken) > 0;
    }

    public async Task<string?> GetSetting(int vehicleId, int settingId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetSetting));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("SettingId", settingId);
        }

        var query = _db.VehicleSettings
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.SettingId == settingId)
            .Select(x => x.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<VehicleUserAccessData?> TryAddUserAccess(int vehicleId, int userId, int accessType, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryAddUserAccess));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessType", accessType);
        }

        var vehicleUserAccessData = new VehicleUserAccessData
        {
            VehicleId = vehicleId,
            UserId = userId,
            AccessType = accessType,
            Metadata = metadata,
        };

        try
        {
            _db.VehicleUserAccess.Add(vehicleUserAccessData);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return vehicleUserAccessData;
    }
    
    public async Task<VehicleGroupAccessData?> TryAddGroupAccess(int vehicleId, int groupId, int accessType, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryAddGroupAccess));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("GroupId", groupId);
            activity.AddTag("AccessType", accessType);
        }

        var vehicleGroupAccessData = new VehicleGroupAccessData
        {
            VehicleId = vehicleId,
            GroupId = groupId,
            AccessType = accessType,
            Metadata = metadata,
        };

        try
        {
            _db.VehicleGroupAccesses.Add(vehicleGroupAccessData);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return vehicleGroupAccessData;
    }

    public async Task<bool> SetUserAccessMetadata(int vehicleId, int userId, int accessType, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetUserAccessMetadata));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.UserId == userId && x.AccessType == accessType);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Metadata, metadata), cancellationToken) > 0;
    }
    
    public async Task<bool> SetGroupAccessMetadata(int vehicleId, int groupId, int accessType, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetGroupAccessMetadata));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("GroupId", groupId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleGroupAccesses
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.GroupId == groupId && x.AccessType == accessType);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Metadata, metadata), cancellationToken) > 0;
    }
    
    public async Task<bool> RemoveUserAccess(int vehicleId, int userId, int accessType, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveUserAccess));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.UserId == userId && x.AccessType == accessType);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }
    
    public async Task<bool> RemoveGroupAccess(int vehicleId, int groupId, int accessType, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveGroupAccess));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("GroupId", groupId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleGroupAccesses
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.GroupId == groupId && x.AccessType == accessType);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }
    
    public async Task<bool> RemoveAllUserAccess(int vehicleId, int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveAllUserAccess));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("UserId", userId);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.UserId == userId);

        return await query.ExecuteDeleteAsync(cancellationToken) > 0;
    }
    
    public async Task<bool> RemoveAllGroupAccess(int vehicleId, int groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveAllGroupAccess));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.VehicleGroupAccesses
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.GroupId == groupId);

        return await query.ExecuteDeleteAsync(cancellationToken) > 0;
    }

    public async Task<bool> HasUserAccess(int vehicleId, int userId, int accessType, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(HasUserAccess));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("UserId", userId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleUserAccess
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.UserId == userId && x.AccessType == accessType);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasGroupAccessTo(int vehicleId, int groupId, int accessType, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetUserAccessMetadata));

        if (activity != null)
        {
            activity.AddTag("VehicleId", vehicleId);
            activity.AddTag("GroupId", groupId);
            activity.AddTag("AccessType", accessType);
        }

        var query = _db.VehicleGroupAccesses
            .TagWithSource(nameof(VehiclesRepository))
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.GroupId == groupId && x.AccessType == accessType);

        return await query.AnyAsync(cancellationToken);
    }

    private IQueryable<VehicleData> CreateQueryBase()
    {
        var query = _db.Vehicles
            .TagWithSource(nameof(VehiclesRepository))
            .Where(x => !x.IsRemoved);

        return query;
    }

    public static readonly ActivitySource Activity = new("RealmCore.VehicleRepository", "1.0.0");
}
