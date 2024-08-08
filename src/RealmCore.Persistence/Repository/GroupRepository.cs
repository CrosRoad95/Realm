namespace RealmCore.Persistence.Repository;

public sealed class GroupRepository
{
    private readonly IDb _db;

    public GroupRepository(IDb db)
    {
        _db = db;
    }

    public async Task<GroupMemberData?> GetGroupMembersByUserIdAndGroupId(int groupId, int userId, int[]? kinds = null, CancellationToken cancellationToken = default)
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

    public async Task<GroupData?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByName));

        if (activity != null)
        {
            activity.AddTag("Id", id);
        }

        var query = CreateQueryBase()
            .IncludeAll()
            .Where(x => x.Id == id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupData?> GetByName(string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetByName));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        var query = CreateQueryBase()
            .IncludeAll()
            .Where(x => x.Name == name);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<GroupData[]> Search(string? name = null, int limit = 10, byte[]? kinds = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Search));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        IQueryable<GroupData> query = CreateQueryBase()
            .Include(x => x.Members);

        if(!string.IsNullOrEmpty(name))
            query = query.Where(x => x.Name.ToLower().Contains(name.ToLower()));

        if(kinds != null)
            query = query.Where(x => x.Kind != null && kinds.Contains(x.Kind.Value));

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

    public async Task<bool> ExistsByName(string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(ExistsByName));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Name.ToLower() == name.ToLower());

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameOrShortcut(string name, string shortcut, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(ExistsByNameOrShortcut));

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

    public async Task<bool> ExistsByShortcut(string shortcut, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(ExistsByShortcut));

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

    public async Task<bool> TryAddMember(int groupId, int userId, DateTime createdAt, int? roleId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryAddMember));

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
    
    public async Task<bool> IsUserInGroup(int groupId, int userId, CancellationToken cancellationToken = default)
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

    public async Task<bool> TryRemoveMember(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryRemoveMember));

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
    
    public async Task<GroupRoleData> CreateRole(int groupId, string name, int[] permissions, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(TryRemoveMember));

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

    public async Task<bool> SetRolePermissions(int roleId, int[] permissions, CancellationToken cancellationToken = default)
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
    
    public async Task<bool> SetMemberRole(int groupId, int userId, int roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        var groupMember = await query.FirstOrDefaultAsync(cancellationToken);

        if (groupMember == null)
            return false;

        if(groupMember.RoleId == roleId)
            return false;

        groupMember.RoleId = roleId;
        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();

        return true;
    }
    
    public async Task<int[]> GetGroupRoles(int groupId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId)
            .Select(x => x.Id);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

    public async Task<int[]> GetRolePermissions(int roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRolesPermissions
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupRoleId == roleId)
            .Select(x => x.PermissionId);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

    public async Task<int?> GetGroupIdByRoleId(int roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == roleId)
            .Select(x => x.GroupId);

        var groupId = await query.FirstOrDefaultAsync(cancellationToken);

        return groupId;
    }

    private IQueryable<GroupData> CreateQueryBase() => _db.Groups.TagWithSource(nameof(GroupRepository));

    public static readonly ActivitySource Activity = new("RealmCore.GroupRepository", "1.0.0");
}
