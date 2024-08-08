namespace RealmCore.Persistence.Repository;

public record struct GroupId(int id)
{
    public static implicit operator int(GroupId id) => id;
    public static implicit operator GroupId(int id) => id;
}

public record struct GroupRoleId(int id)
{
    public static implicit operator int(GroupRoleId id) => id;
    public static implicit operator GroupRoleId(int id) => id;
}

public record struct GroupMemberId(int id)
{
    public static implicit operator int(GroupMemberId id) => id;
    public static implicit operator GroupMemberId(int id) => id;
}

public sealed class GroupRepository
{
    private readonly IDb _db;

    public GroupRepository(IDb db)
    {
        _db = db;
    }

    #region Create
    public async Task<GroupData> Create(string name, string? shortcut = null, byte kind = 1, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Create));

        if (activity != null)
        {
            activity.AddTag("Name", name);
            activity.AddTag("Shortcut", shortcut);
            activity.AddTag("Kind", kind);
        }

        var group = new GroupData
        {
            Name = name,
            Shortcut = shortcut,
            Kind = kind,
        };
        _db.Groups.Add(group);

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();

        return group;
    }

    public async Task<bool> AddMember(GroupId groupId, int userId, DateTime createdAt, GroupRoleId? roleId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(AddMember));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
            activity.AddTag("RoleId", roleId);
        }

        var groupMember = new GroupMemberData
        {
            GroupId = groupId,
            UserId = userId,
            CreatedAt = createdAt,
            RoleId = roleId,
            Metadata = metadata
        };
        _db.GroupMembers.Add(groupMember);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _db.ChangeTracker.Clear();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<GroupRoleData> CreateRole(GroupId groupId, string name, int[] permissions, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveMember));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("Name", name);
            activity.AddTag("Permissions", permissions);
        }

        var groupRole = new GroupRoleData
        {
            GroupId = groupId,
            Name = name,
            Permissions = permissions.Select(x => new GroupRolePermissionData
            {
                PermissionId = x
            }).ToArray()
        };

        _db.GroupsRoles.Add(groupRole);
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();

        return groupRole;
    }

    public async Task<bool> SetRolePermissions(GroupRoleId roleId, int[] permissions, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == roleId)
            .Include(x => x.Permissions);

        var groupRole = await query.FirstOrDefaultAsync(cancellationToken);

        if (groupRole == null)
            return false;

        groupRole.Permissions = permissions.Select(x => new GroupRolePermissionData
        {
            PermissionId = x
        }).ToList();
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();

        return true;
    }
    #endregion

    #region Read
    public async Task<GroupMemberData?> GetGroupMemberByUserIdAndGroupId(GroupId groupId, int userId, int[]? kinds = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupMembersByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .Include(x => x.Role)
            .ThenInclude(x => x!.Permissions)
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        if (kinds != null)
        {
            query = query.Where(x => x.Group!.Kind != null && kinds.Contains(x.Group!.Kind.Value));
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupMemberData[]> GetGroupMembersByUserId(int userId, int[]? kinds = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupMembersByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = CreateQueryBase()
            .Include(x => x.Members)
            .ThenInclude(x => x.Group)
            .Where(x => x.Members.Any(x => x.UserId == userId));

        if (kinds != null)
        {
            query = query.Where(x => x.Kind != null && kinds.Contains(x.Kind.Value));
        }

        var query2 = query.SelectMany(x => x.Members);

        return await query2.ToArrayAsync(cancellationToken);
    }

    public async Task<GroupData?> GetGroupById(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupByName));

        if (activity != null)
        {
            activity.AddTag("Id", groupId);
        }

        var query = CreateQueryBase()
            .IncludeAll()
            .Where(x => x.Id == groupId);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupData?> GetGroupByName(string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupByName));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        var query = CreateQueryBase()
            .IncludeAll()
            .Where(x => x.Name == name);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupData[]> SearchGroups(string? name = null, int limit = 10, byte[]? kinds = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SearchGroups));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        IQueryable<GroupData> query = CreateQueryBase()
            .Include(x => x.Members);

        if (!string.IsNullOrEmpty(name))
            query = query.Where(x => x.Name.ToLower().Contains(name.ToLower()));

        if (kinds != null)
            query = query.Where(x => x.Kind != null && kinds.Contains(x.Kind.Value));

        query = query.Take(limit);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<GroupData?> GetGroupByNameOrShortcut(string name, string shortcut, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupByNameOrShortcut));

        if (activity != null)
        {
            activity.AddTag("Name", name);
            activity.AddTag("Shortcut", shortcut);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Include(x => x.Members)
            .Where(x => x.Name == name || x.Shortcut == shortcut);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> GroupExistsByName(string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GroupExistsByName));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Name.ToLower() == name.ToLower());

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> GroupExistsByNameOrShortcut(string name, string shortcut, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GroupExistsByNameOrShortcut));

        if (activity != null)
        {
            activity.AddTag("Name", name);
            activity.AddTag("Shortcut", shortcut);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Name.ToLower() == name.ToLower() || x.Shortcut == null || x.Shortcut.ToLower() == shortcut.ToLower());
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> GroupExistsByShortcut(string shortcut, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GroupExistsByShortcut));

        if (activity != null)
        {
            activity.AddTag("Shortcut", shortcut);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Shortcut != null && x.Shortcut.ToLower() == shortcut.ToLower());

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetGroupIdByName(string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupIdByName));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Name == name)
            .Select(x => x.Id);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsUserInGroup(GroupId groupId, int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(IsUserInGroup));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int[]> GetGroupRoles(GroupId groupId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId)
            .Select(x => x.Id);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

    public async Task<int[]> GetRolePermissions(GroupRoleId roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRolesPermissions
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupRoleId == roleId)
            .Select(x => x.PermissionId);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

    public async Task<GroupId?> GetGroupIdByRoleId(GroupRoleId roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == roleId)
            .Select(x => x.GroupId);

        var groupId = await query.FirstOrDefaultAsync(cancellationToken);

        return groupId;
    }
    #endregion

    #region Update
    public async Task<bool> SetMemberRole(GroupId groupId, int userId, GroupRoleId roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        var groupMember = await query.FirstOrDefaultAsync(cancellationToken);

        if (groupMember == null)
            return false;

        if (groupMember.RoleId == roleId)
            return false;

        groupMember.RoleId = roleId;
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();

        return true;
    }

    public async Task<bool> SetRoleName(GroupRoleId roleId, string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetRoleName));

        if (activity != null)
        {
            activity.AddTag("RoleId", roleId);
            activity.AddTag("Name", name);
        }

        var query = _db.GroupsRoles
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == roleId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Name, name), cancellationToken) == 1;
    }
    #endregion

    #region Delete
    public async Task<bool> RemoveMember(GroupId groupId, int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveMember));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }

    public async Task<bool> RemoveRole(GroupRoleId roleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveRole));

        if (activity != null)
        {
            activity.AddTag("RoleId", roleId);
        }

        var query = _db.GroupsRoles
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == roleId);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }
    #endregion
    
    private IQueryable<GroupData> CreateQueryBase() => _db.Groups.TagWithSource(nameof(GroupRepository));

    public static readonly ActivitySource Activity = new("RealmCore.GroupRepository", "1.0.0");
}
