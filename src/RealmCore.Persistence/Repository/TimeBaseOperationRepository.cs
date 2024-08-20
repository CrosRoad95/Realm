using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        using var activity = Activity.StartActivity(nameof(CreateGroup));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = _db.TimeBaseOperationsGroups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TimeBaseOperationGroupData> CreateGroup(int category, int limit, string? metadata = null, CancellationToken cancellationToken = default)
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
            Metadata = metadata
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

    public async Task<TimeBaseOperationDataGroupUserData?> CreateOperationForUser(int groupId, int userId, int type, int status, DateTime startDateTime, DateTime endDateTime, string? input = null, string? output = null, string? metadata = null, CancellationToken cancellationToken = default)
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


        var group = await GetGroupById(groupId, cancellationToken);
        if (group == null)
            return null;

        var timeBaseOperationDataGroupUser = new TimeBaseOperationDataGroupUserData
        {
            GroupId = groupId,
            UserId = userId,
            Metadata = metadata,
            Operation = new TimeBaseOperationData
            {
                Type = type,
                Status = status,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Input = input,
                Output = output,
            }
        };

        try
        {
            _db.TimeBaseOperationsGroupsUsers.Add(timeBaseOperationDataGroupUser);
            await _db.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        timeBaseOperationDataGroupUser.Group = group;

        return timeBaseOperationDataGroupUser;
    }

    public async Task<TimeBaseOperationDataGroupUserData[]> GetOperationsByUserIdAndCategory(int userId, int category, CancellationToken cancellationToken = default)
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
            .Include(x => x.Operation)
            .Include(x => x.Group)
            .Where(x => x.UserId == userId && x.Group!.Category == category);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> DeleteOperation(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetOperationsByUserIdAndCategory));

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

    public static readonly ActivitySource Activity = new("RealmCore.TimeBaseOperationRepository", "1.0.0");
}
