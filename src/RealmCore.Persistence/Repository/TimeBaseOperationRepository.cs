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
    
    public async Task<TimeBaseOperationGroupData[]> GetGroupsByUserIdAndCategoryId(int userId, int categoryId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupsByUserIdAndCategoryId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("CategoryId", categoryId);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Operations)
            .Where(x => x.Category == categoryId && x.GroupUserOperations != null && x.GroupUserOperations.Any(y => y.UserId == userId));

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
    
    public async Task<TimeBaseOperationGroupData[]> GetGroupsByUserId(int userId, bool withOperations = true, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupsByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        IQueryable<TimeBaseOperationGroupUserData> query;

        if (withOperations)
        {
            query = _db.TimeBaseOperationsGroupsUsers
                .TagWithSource(nameof(GroupRepository))
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Group)
                .ThenInclude(x => x!.Operations);
        }
        else
        {
            query = _db.TimeBaseOperationsGroupsUsers
                .TagWithSource(nameof(GroupRepository))
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Group);
        }

        var query2 = query
            .Where(x => x.UserId == userId)
            .Select(x => x.Group);

        return await query2.ToArrayAsync(cancellationToken);
    }

    public async Task<TimeBaseOperationGroupUserData> CreateGroupForUser(int userId, int category, int limit, string? groupUserMetadata = null, string? groupMetadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateGroupForUser));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Category", category);
            activity.AddTag("Limit", limit);
        }

        var timeBaseOperationGroup = new TimeBaseOperationGroupUserData
        {
            UserId = userId,
            Metadata = groupUserMetadata,
            Group = new TimeBaseOperationGroupData
            {
                Category = category,
                Limit = limit,
                Metadata = groupMetadata,
            }
        };

        try
        {
            _db.TimeBaseOperationsGroupsUsers.Add(timeBaseOperationGroup);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return timeBaseOperationGroup;
    }

    public async Task<TimeBaseOperationData?> CreateOperationForUser(int groupId, int userId, int type, int status, DateTime startDateTime, DateTime endDateTime, string? input = null, string? output = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateOperationForUser));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
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

    public async Task<TimeBaseOperationGroupUserData[]> GetOperationsByUserIdAndCategory(int userId, int category, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetOperationsByUserIdAndCategory));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
            activity.AddTag("Type", category);
        }

        var query = _db.TimeBaseOperationsGroupsUsers
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .ThenInclude(x => x!.Operations)
            .Where(x => x.UserId == userId && x.Group!.Category == category);

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
