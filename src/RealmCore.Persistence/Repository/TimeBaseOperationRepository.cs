namespace RealmCore.Persistence.Repository;

public sealed class TimeBaseOperationRepository
{
    private readonly IDb _db;

    public TimeBaseOperationRepository(IDb db)
    {
        _db = db;
    }

    public async Task<TimeBaseOperationGroupData?> GetGroupById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupById));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Operations)
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<TimeBaseOperationGroupData[]> GetGroupsByCategoryId(int categoryId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupsByCategoryId));

        if (activity != null)
        {
            activity.AddTag("CategoryId", categoryId);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Operations)
            .Where(x => x.Category == categoryId);

        return await query.ToArrayAsync(cancellationToken);
    }
    
    public async Task<int?> GetGroupLimitById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupLimitById));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id)
            .Select(x => x.Limit);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<TimeBaseOperationGroupData[]> GetGroupsByBusinessId(int businessId, bool withOperations = true, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupsByBusinessId));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
        }

        IQueryable<TimeBaseOperationGroupBusinessData> query;

        if (withOperations)
        {
            query = _db.TimeBaseOperationGroupBusinesses
                .TagWithSource(nameof(GroupRepository))
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.OperationGroup)
                .ThenInclude(x => x!.Businesses)
                .ThenInclude(x => x!.Business)
                .Include(x => x.OperationGroup)
                .ThenInclude(x => x!.Operations);
        }
        else
        {
            query = _db.TimeBaseOperationGroupBusinesses
                .TagWithSource(nameof(GroupRepository))
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.OperationGroup)
                .ThenInclude(x => x!.Businesses)
                .ThenInclude(x => x!.Business);
        }

        var query2 = query
            .Where(x => x.BusinessId == businessId)
            .Select(x => x.OperationGroup);

        return await query2.ToArrayAsync(cancellationToken);
    }

    public async Task<TimeBaseOperationGroupBusinessData> CreateGroupForBusiness(int businessId, int category, int limit, string? groupMetadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateGroupForBusiness));

        if (activity != null)
        {
            activity.AddTag("BusinessId", businessId);
            activity.AddTag("Category", category);
            activity.AddTag("Limit", limit);
        }

        var baseOperationGroupBusinessData = new TimeBaseOperationGroupBusinessData
        {
            BusinessId = businessId,
            OperationGroup = new TimeBaseOperationGroupData
            {
                Category = category,
                Limit = limit,
                Metadata = groupMetadata,
            }
        };

        try
        {
            _db.TimeBaseOperationGroupBusinesses.Add(baseOperationGroupBusinessData);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return baseOperationGroupBusinessData;
    }
    
    public async Task<TimeBaseOperationGroupData> CreateGroup(int category, int limit, string? groupMetadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateGroup));

        if (activity != null)
        {
            activity.AddTag("Category", category);
            activity.AddTag("Limit", limit);
        }

        var timeBaseOperationGroup = new TimeBaseOperationGroupData
        {
            Category = category,
            Limit = limit,
            Metadata = groupMetadata,
        };

        try
        {
            _db.TimeBaseOperationsGroups.Add(timeBaseOperationGroup);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return timeBaseOperationGroup;
    }

    public async Task<TimeBaseOperationData?> CreateOperation(int groupId, int type, int status, DateTime startDateTime, DateTime endDateTime, string? input = null, string? output = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateOperation));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("Type", type);
            activity.AddTag("StartDateTime", startDateTime);
            activity.AddTag("EndDateTime", endDateTime);
        }

        var groupLimit = await GetGroupLimitById(groupId, cancellationToken);
        if (groupLimit == null)
            return null;

        var count = await CountOperationsByGroupId(groupId, cancellationToken);
        if (count >= groupLimit)
            return null;

        var operation = new TimeBaseOperationData
        {
            Type = type,
            GroupId = groupId,
            Status = status,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            Input = input,
            Output = output,
            Metadata = metadata,
        };

        try
        {
            _db.TimeBaseOperations.Add(operation);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return operation;
    }

    public async Task<int> CountOperationsByGroupId(int groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CountOperationsByGroupId));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.TimeBaseOperations
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.GroupId == groupId);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<TimeBaseOperationGroupBusinessData[]> GetOperationsByUserIdAndBusinessCategory(int userId, int category, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetOperationsByUserIdAndBusinessCategory));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Category", category);
        }

        var query = _db.TimeBaseOperationGroupBusinesses
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.OperationGroup)
            .ThenInclude(x => x!.Operations)
            .Where(x => x.Business!.Category == category && x.Business.Users.Any(x => x.UserId == userId));

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<TimeBaseOperationData[]> GetOperationsByGroupId(int groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetOperationsByGroupId));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Operations)
            .Where(x => x.Id == groupId)
            .SelectMany(x => x.Operations);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> SetStatus(int id, int status, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetStatus));

        if (activity != null)
        {
            activity.AddTag("Id", id);
            activity.AddTag("Status", status);
        }

        var query = _db.TimeBaseOperations
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Status, status), cancellationToken) == 1;
    }
    
    public async Task<bool> DeleteOperation(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(DeleteOperation));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.TimeBaseOperations
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == id);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }

    public async Task<string?> GetGroupMetadata(int groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetGroupMetadata));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == groupId)
            .Select(x => x.Metadata);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<bool> SetGroupMetadata(int groupId, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetGroupMetadata));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == groupId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Metadata, metadata), cancellationToken) == 1;
    }

    public async Task<string?> GetOperationMetadata(int operationId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetGroupMetadata));

        if (activity != null)
        {
            activity.AddTag("OperationId", operationId);
        }

        var query = _db.TimeBaseOperations
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == operationId)
            .Select(x => x.Metadata);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<bool> SetOperationMetadata(int operationId, string? metadata, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetOperationMetadata));

        if (activity != null)
        {
            activity.AddTag("OperationId", operationId);
        }

        var query = _db.TimeBaseOperations
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == operationId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Metadata, metadata), cancellationToken) == 1;
    }

    public static readonly ActivitySource Activity = new("RealmCore.TimeBaseOperationRepository", "1.0.0");
}
