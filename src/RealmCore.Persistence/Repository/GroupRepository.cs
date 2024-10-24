﻿namespace RealmCore.Persistence.Repository;

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
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }

        return group;
    }

    public async Task<bool> AddMember(GroupId groupId, int userId, DateTime createdAt, GroupRoleId? roleId = null, string? metadata = null, bool force = false, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(AddMember));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
            activity.AddTag("RoleId", roleId);
        }

        if (!force)
        {
            var isInGroup = await IsUserInGroup(groupId, userId, cancellationToken);
            if (isInGroup)
                return false;
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
        using var activity = Activity.StartActivity(nameof(SetRolePermissions));

        if (activity != null)
        {
            activity.AddTag("RoleId", roleId);
            activity.AddTag("Permissions", permissions);
        }

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

    public async Task<GroupMemberData?> GetGroupMemberByUserIdAndGroupId(GroupId groupId, int userId, int[]? kinds = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupMemberByUserIdAndGroupId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .ThenInclude(x => x!.Upgrades)
            .Include(x => x.Role)
            .ThenInclude(x => x!.Permissions)
            .AsSplitQuery()
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

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Include(x => x.Group)
            .Include(x => x.Role)
            .ThenInclude(x => x!.Permissions)
            .Where(x => x.UserId == userId);

        if (kinds != null)
        {
            query = query.Where(x => x.Group!.Kind != null && kinds.Contains(x.Group.Kind.Value));
        }

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<GroupData?> GetGroupById(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupByName));

        if (activity != null)
        {
            activity.AddTag("Id", groupId);
        }

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
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
            .AsNoTrackingWithIdentityResolution()
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

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .IncludeAll()
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

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .IncludeAll()
            .Where(x => x.Name == name || x.Shortcut == shortcut);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupData?> GetGroupByGroupRoleId(GroupRoleId groupRoleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupByGroupRoleId));

        if (activity != null)
        {
            activity.AddTag("GroupRoleId", groupRoleId);
        }

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .IncludeAll()
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

        var query = CreateQueryBase()
            .IncludeAll()
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
            .AsNoTracking()
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

        var query = CreateQueryBase()
            .AsNoTracking()
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

        var query = CreateQueryBase()
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Shortcut != null && x.Shortcut.ToLower() == shortcut.ToLower());

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int?> GetGroupIdByName(string name, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupIdByName));

        if (activity != null)
        {
            activity.AddTag("Name", name);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Name == name)
            .Select(x => x.Id);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<byte?> GetGroupKindById(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupIdByName));

        if (activity != null)
        {
            activity.AddTag("Name", groupId);
        }

        var query = CreateQueryBase()
            .AsNoTracking()
            .Where(x => x.Id == groupId.id)
            .Select(x => x.Kind);

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
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId.id && x.UserId == userId);

        return await query.AnyAsync(cancellationToken);
    }
    
    public async Task<bool> IsUserInGroupOfKind(byte groupKind, int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(IsUserInGroupOfKind));

        if (activity != null)
        {
            activity.AddTag("GroupKind", groupKind);
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsMembers
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Group!.Kind == groupKind && x.UserId == userId);

        return await query.AnyAsync(cancellationToken);
    }
    
    public async Task<bool> IsInAnyGroup(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(IsInAnyGroup));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsMembers
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.UserId == userId);

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
            .TagWithSource(nameof(GroupRepository))
            .AsNoTracking()
            .Include(x => x.Members)
            .Include(x => x.Permissions)
            .Where(x => x.GroupId == groupId.id);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

    public async Task<int[]> GetRolePermissions(GroupRoleId roleId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupsRolesPermissions
            .TagWithSource(nameof(GroupRepository))
            .AsNoTracking()
            .Where(x => x.GroupRoleId == roleId.id)
            .Select(x => x.PermissionId);

        var permissions = await query.ToArrayAsync(cancellationToken);

        return permissions;
    }

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

    public async Task<string?> GetGroupName(GroupId groupId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupName));

        if (activity != null)
        {
            activity.AddTag("RoleId", groupId);
        }

        var query = _db.Groups
            .AsNoTracking()
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == groupId.id)
            .Select(x => x.Name);

        return await query.FirstOrDefaultAsync(cancellationToken);
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
    
    public async Task<bool> RemoveAllJoinRequestsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(RemoveAllJoinRequestsByUserId));

        if (activity != null)
        {
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsJoinRequests
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.UserId == userId);

        return await query.ExecuteDeleteAsync(cancellationToken) > 0;
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

    public async Task<int[]> GetGroupMemberPermissions(GroupId groupId, int userId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetGroupMemberPermissions));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
        }

        var query = _db.GroupsMembers
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.GroupId == groupId.id && x.UserId == userId)
            .SelectMany(x => x.Role!.Permissions.Select(y => y.PermissionId));

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<string?> GetRoleName(GroupRoleId groupRoleId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetRoleName));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupRoleId);
        }

        var query = _db.GroupsRoles
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Id == groupRoleId.id)
            .Select(x => x.Name);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<GroupData[]> GetAll(int page, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetAll));

        if (activity != null)
        {
            activity.AddTag("Page", page);
            activity.AddTag("PageSize", pageSize);
        }

        page = Math.Max(page, 1);

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToArrayAsync(cancellationToken);
    }

    public async Task<bool> GiveMoney(GroupId groupId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        using var activity = Activity.StartActivity(nameof(GiveMoney));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId.id);
            activity.AddTag("Amount", amount);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == groupId.id);

        var groupData = await query.FirstOrDefaultAsync(cancellationToken);
        if (groupData == null)
            return false;

        try
        {
            groupData.Money += amount;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TakeMoney(GroupId groupId, decimal amount, CancellationToken cancellationToken = default)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        using var activity = Activity.StartActivity(nameof(TakeMoney));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId.id);
            activity.AddTag("Amount", amount);
        }

        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Id == groupId.id);

        var groupData = await query.FirstOrDefaultAsync(cancellationToken);
        if (groupData == null)
            return false;

        try
        {
            groupData.Money -= amount;
            if (groupData.Money < 0)
                return false;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> AddUpgrade(GroupId groupId, int upgradeId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(AddUpgrade));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UpgradeId", upgradeId);
        }

        var upgradeData = new GroupUpgradeData
        {
            GroupId = groupId.id,
            UpgradeId = upgradeId,
        };

        try
        {
            _db.GroupsUpgrades.Add(upgradeData);
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
    
    public async Task<bool> IncreaseStatistic(GroupId groupId, int userId, int statisticId, DateOnly date, float value, CancellationToken cancellationToken = default)
    {
        if (value < 0)
            return false;

        using var activity = Activity.StartActivity(nameof(IncreaseStatistic));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
            activity.AddTag("StatisticId", statisticId);
            activity.AddTag("Date", date);
            activity.AddTag("Value", value);
        }

        var groupMemberQuery = _db.GroupsMembers.Where(x => x.GroupId == groupId.id && x.UserId == userId)
            .Select(x => x.Id);

        var groupMemberId = await groupMemberQuery.FirstOrDefaultAsync(cancellationToken);
        if (groupMemberId == 0)
            return false;

        GroupMemberStatisticData? statistic = null;

        var query = _db.GroupsMembersStatistics.Where(x => x.GroupMemberId == groupMemberId && x.Date == date && x.StatisticId == statisticId);

        statistic = await query.FirstOrDefaultAsync(cancellationToken);

        try
        {
            if(statistic == null)
            {
                _db.GroupsMembersStatistics.Add(new GroupMemberStatisticData
                {
                    GroupMemberId = groupMemberId,
                    Date = date,
                    StatisticId = statisticId,
                    Value = value
                });
            }
            else
            {
                statistic.Value += value;
            }
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
    
    public async Task<GroupMemberStatisticData[]> GetStatistics(GroupId groupId, int[]? statisticsIds = null, DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetStatistics));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("StatisticsIds", statisticsIds);
            activity.AddTag("Date", date);
        }

        var query = _db.GroupsMembers.Where(x => x.GroupId == groupId.id)
            .SelectMany(x => x.Statistics)
            .Include(x => x.GroupMember)
            .Where(x => (statisticsIds == null || statisticsIds.Contains(x.StatisticId)) && (date == null || x.Date == date));

        try
        {
            return await query.ToArrayAsync(cancellationToken);
        }
        catch (Exception)
        {
            return [];
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }
    
    public async Task<GroupMemberStatisticData[]> GetStatistics(GroupId groupId, DateOnly from, DateOnly to, int[]? statisticsIds, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetStatistics));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("StatisticsIds", statisticsIds);
            activity.AddTag("From", from);
            activity.AddTag("To", to);
        }

        var query = _db.GroupsMembers.Where(x => x.GroupId == groupId.id)
            .SelectMany(x => x.Statistics)
            .Include(x => x.GroupMember)
            .Where(x => (statisticsIds == null || statisticsIds.Contains(x.StatisticId)) && x.Date >= from && x.Date <= to);

        try
        {
            return await query.ToArrayAsync(cancellationToken);
        }
        catch (Exception)
        {
            return [];
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }
    
    public async Task<GroupMemberStatisticData[]> GetStatisticsByUserId(GroupId groupId, int userId, DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetStatisticsByUserId));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
        }

        var groupMemberQuery = _db.GroupsMembers.Where(x => x.GroupId == groupId.id && x.UserId == userId)
            .Select(x => x.Id);

        var groupMemberId = await groupMemberQuery.FirstOrDefaultAsync(cancellationToken);
        if (groupMemberId == 0)
            return [];

        var query = _db.GroupsMembersStatistics.Where(x => x.GroupMemberId == groupMemberId && (date == null || x.Date == date));

        try
        {
            return await query.ToArrayAsync(cancellationToken);
        }
        catch (Exception)
        {
            return [];
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }
    
    public async Task<GroupMemberStatisticData[]> GetStatisticsByUserId(GroupId groupId, int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetStatisticsByUserId));

        if (activity != null)
        {
            activity.AddTag("GroupId", groupId);
            activity.AddTag("UserId", userId);
        }

        var groupMemberQuery = _db.GroupsMembers.Where(x => x.GroupId == groupId.id && x.UserId == userId)
            .Select(x => x.Id);

        var groupMemberId = await groupMemberQuery.FirstOrDefaultAsync(cancellationToken);
        if (groupMemberId == 0)
            return [];

        var query = _db.GroupsMembersStatistics.Where(x => x.GroupMemberId == groupMemberId && x.Date >= from && x.Date <= to);

        try
        {
            return await query.ToArrayAsync(cancellationToken);
        }
        catch (Exception)
        {
            return [];
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    private IQueryable<GroupData> CreateQueryBase() => _db.Groups.TagWithSource(nameof(GroupRepository));

    public static readonly ActivitySource Activity = new("RealmCore.GroupRepository", "1.0.0");
}
