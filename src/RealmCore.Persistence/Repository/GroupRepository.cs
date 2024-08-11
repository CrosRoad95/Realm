using System.Xml.Linq;

namespace RealmCore.Persistence.Repository;

public record struct GroupId(int id)
{
    public static implicit operator int(GroupId id) => id.id;
    public static implicit operator GroupId(int id) => new(id);
}

public record struct GroupRoleId(int id)
{
    public static implicit operator int(GroupRoleId id) => id.id;
    public static implicit operator GroupRoleId(int id) => new(id);
}

public record struct GroupMemberId(int id)
{
    public static implicit operator int(GroupMemberId id) => id.id;
    public static implicit operator GroupMemberId(int id) => new(id);
}

public sealed class GroupRepository
{
    private readonly IDb _db;

    public GroupRepository(IDb db)
    {
        _db = db;
    }

    #region Create
    public async Task<GroupData?> Create(string name, DateTime createdAt, string? shortcut = null, byte kind = 1, CancellationToken cancellationToken = default)
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
            CreatedAt = createdAt
        };

        try
        {
            _db.Groups.Add(group);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return null;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

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
        _db.GroupsMembers.Add(groupMember);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<GroupRoleData?> CreateRole(GroupId groupId, string name, int[] permissions, CancellationToken cancellationToken = default)
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

        try
        {
            _db.GroupsRoles.Add(groupRole);
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

        return groupRole;
    }

    public async Task<bool> SetRolePermissions(GroupRoleId roleId, int[] permissions, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == roleId.id)
            .Include(x => x.Permissions);

        var groupRole = await query.FirstOrDefaultAsync(cancellationToken);

        if (groupRole == null)
            return false;

        groupRole.Permissions = permissions.Select(x => new GroupRolePermissionData
        {
            PermissionId = x
        }).ToList();

        try
        {

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

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

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .Include(x => x.Role)
            .ThenInclude(x => x!.Permissions)
            .Where(x => x.GroupId == groupId.id && x.UserId == userId);

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
            .Where(x => x.Id == groupId.id);

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
            .Include(x => x.Members)
            .ThenInclude(x => x.Role)
            .ThenInclude(x => x!.Permissions)
            .Include(x => x.Settings)
            .AsSplitQuery();

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

    public async Task<GroupData?> GetGroupByGroupId(GroupRoleId groupRoleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupByGroupId));

        if (activity != null)
        {
            activity.AddTag("GroupRoleId", groupRoleId);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Include(x => x.Members)
            .Where(x => x.Roles.Any(y => y.Id == groupRoleId.id));

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupId?> GetGroupIdByRoleId(GroupRoleId groupRoleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupIdByRoleId));

        if (activity != null)
        {
            activity.AddTag("GroupRoleId", groupRoleId);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Include(x => x.Members)
            .Where(x => x.Roles.Any(y => y.Id == groupRoleId.id))
            .Select(x => x.Id);

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

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id && x.UserId == userId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int[]> GetGroupRolesIds(GroupId groupId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id)
            .Select(x => x.Id);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

    public async Task<GroupRoleData[]> GetGroupRoles(GroupId groupId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRoles
            .Include(x => x.Members)
            .Include(x => x.Permissions)
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

    public async Task<int[]> GetRolePermissions(GroupRoleId roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRolesPermissions
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupRoleId == roleId.id)
            .Select(x => x.PermissionId);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }
    #endregion

    #region Update
    public async Task<bool> SetMemberRole(GroupId groupId, int userId, GroupRoleId? roleId = null, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id && x.UserId == userId);

        var groupMember = await query.FirstOrDefaultAsync(cancellationToken);

        if (groupMember == null)
            return false;

        if (groupMember.RoleId == roleId)
            return false;

        groupMember.RoleId = roleId;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return true;
    }

    public async Task<bool> RemoveAllMembersFromRole(GroupRoleId roleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveAllMembersFromRole));

        if (activity != null)
        {
            activity.AddTag("RoleId", roleId);
        }

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.RoleId == roleId.id);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.RoleId, (int?)null), cancellationToken) > 0;
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
            .Where(x => x.Id == roleId.id);

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

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id && x.UserId == userId);

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
            .Where(x => x.Id == roleId.id);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }

    public async Task<bool> SetGroupSetting(GroupId groupId, int settingId, string value, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(SetGroupSetting));

        if (activity != null)
        {
            activity.AddTag("RoleId", groupId);
            activity.AddTag("SettingId", settingId);
        }

        var query = _db.GroupsSettings
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id && x.SettingId == settingId);

        var groupSettingData = await query.FirstOrDefaultAsync(cancellationToken);

        try
        {
            if (groupSettingData != null)
            {
                groupSettingData.Value = value;
            }
            else
            {
                _db.GroupsSettings.Add(new GroupSettingData
                {
                    GroupId = groupId.id,
                    SettingId = settingId,
                    Value = value
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
        return true;
    }

    public async Task<string?> GetGroupSetting(GroupId groupId, int settingId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupSetting));

        if (activity != null)
        {
            activity.AddTag("RoleId", groupId);
            activity.AddTag("SettingId", settingId);
        }

        var query = _db.GroupsSettings
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id && x.SettingId == settingId);

        var groupSettingData = await query.FirstOrDefaultAsync(cancellationToken);
        if (groupSettingData != null)
        {
            return groupSettingData.Value;
        }
        return null;
    }

    public async Task<IReadOnlyDictionary<int, string>> GetGroupSettings(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupSettings));

        if (activity != null)
        {
            activity.AddTag("RoleId", groupId);
        }

        var query = _db.GroupsSettings
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id);

        var settings = await query.ToDictionaryAsync(x => x.SettingId, x => x.Value, cancellationToken);
        return settings;
    }

    public async Task<bool> CreateJoinRequest(GroupId groupId, int userId, DateTime createdAt, string? metadata = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateJoinRequest));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
        }

        var joinRequest = new GroupJoinRequestData
        {
            GroupId = groupId.id,
            UserId = userId,
            CreatedAt = createdAt,
            Metadata = metadata ?? "",
        };

        try
        {
            _db.GroupsJoinRequests.Add(joinRequest);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<int> CountJoinRequestsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CountJoinRequestsByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsJoinRequests
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .Where(x => x.UserId == userId);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> CountJoinRequestsByGroupId(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CountJoinRequestsByGroupId));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.GroupsJoinRequests
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .Where(x => x.GroupId == groupId.id);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<GroupJoinRequestData[]> GetJoinRequestsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateJoinRequest));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsJoinRequests
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .Where(x => x.UserId == userId);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<GroupJoinRequestData[]> GetJoinRequestsByGroupId(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(CreateJoinRequest));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.GroupsJoinRequests
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .Where(x => x.GroupId == groupId.id);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> RemoveJoinRequest(GroupId groupId, int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveJoinRequest));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsJoinRequests
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id && x.UserId == userId);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }

    public async Task<GroupMemberData[]> GetGroupMembers(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupMembers));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
        }

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Role)
            .ThenInclude(x => x!.Permissions)
            .Where(x => x.GroupId == groupId.id);

        return await query.ToArrayAsync(cancellationToken);
    }
    #endregion

    private IQueryable<GroupData> CreateQueryBase() => _db.Groups.TagWithSource(nameof(GroupRepository));

    public static readonly ActivitySource Activity = new("RealmCore.GroupRepository", "1.0.0");
}
