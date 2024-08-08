using RealmCore.Persistence.Context;
using RealmCore.Persistence.Repository;
using SlipeServer.Server.Elements;
using static RealmCore.Server.Modules.Players.Groups.GroupsResults;

namespace RealmCore.Server.Modules.Players.Groups;

public static class GroupsResults
{
    public record struct Created(GroupDto group);
    public record struct NameInUse();
    public record struct ShortcutInUse();
}

public sealed class GroupsService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UsersInUse _usersInUse;
    private readonly GroupsManager _groupsManager;
    private readonly ITransactionContext _transactionContext;
    private readonly GroupRepository _groupRepository;

    public event Action<int, RealmPlayer>? MemberAdded;
    public event Action<int, RealmPlayer>? MemberRemoved;
    public event Action<int, RealmPlayer>? MemberChanged;

    public GroupsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, UsersInUse usersInUse, GroupsManager groupsManager)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dateTimeProvider = dateTimeProvider;
        _usersInUse = usersInUse;
        _groupsManager = groupsManager;
        _transactionContext = _serviceProvider.GetRequiredService<ITransactionContext>();
        _groupRepository = _serviceProvider.GetRequiredService<GroupRepository>();
    }

    public async Task<OneOf<Created, NameInUse, ShortcutInUse>> Create(string name, string? shortcut = null, int kind = 0, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (await _groupRepository.GroupExistsByName(name, cancellationToken))
                return new NameInUse();

            if (shortcut != null && await _groupRepository.GroupExistsByShortcut(shortcut, cancellationToken))
                return new ShortcutInUse();

            var groupData = await _groupRepository.Create(name, shortcut, (byte)kind, cancellationToken);
            return new Created(GroupDto.Map(groupData));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupDto?> GetGroupById(int id, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupData = await _groupRepository.GetGroupById(id, cancellationToken);

            return GroupDto.Map(groupData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupDto?> GetGroupByName(string name, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupData = await _groupRepository.GetGroupByName(name, cancellationToken);

            return GroupDto.Map(groupData);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<GroupDto[]> SearchGroups(string name, int limit = 10, byte[]? kinds = null, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var results = await _groupRepository.SearchGroups(name, limit, kinds, cancellationToken);

            return results.Select(x => GroupDto.Map(x)).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<GroupDto?> GetGroupByNameOrShortCut(string name, string shortcut, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupData = await _groupRepository.GetGroupByNameOrShortcut(name, shortcut, cancellationToken);
            if (groupData == null)
                return null;

            return GroupDto.Map(groupData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> GroupExistsByNameOrShortcut(string name, string shortcut, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GroupExistsByNameOrShortcut(name, shortcut, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> AddMember(RealmPlayer player, GroupId groupId, int? roleId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        return await AddMember(player.UserId, groupId, roleId, metadata, cancellationToken);
    }
    
    public async Task<bool> AddMember(int userId, GroupId groupId, int? roleId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        bool added = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            added = await _groupRepository.AddMember(groupId, userId, _dateTimeProvider.Now, roleId, metadata, cancellationToken);

            if (added)
            {
                if (_usersInUse.TryGetPlayerByUserId(userId, out var player))
                {
                    var groupMemberData = await _groupRepository.GetGroupMemberByUserIdAndGroupId(groupId, player.UserId, cancellationToken: cancellationToken);

                    if (groupMemberData == null)
                        return false;

                    if (!player.Groups.AddGroupMember(groupMemberData))
                        return false;

                    _groupsManager.AddPlayerToGroup(groupId, player);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return added;
    }

    public async Task<bool> RemoveMember(int userId, GroupId groupId, CancellationToken cancellationToken = default)
    {
        bool removed = false;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (await _groupRepository.RemoveMember(groupId, userId, cancellationToken))
            {
                removed = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        if(removed && _usersInUse.TryGetPlayerByUserId(userId, out var player))
        {
            var playerRemovedFromGroup = player.Groups.RemoveGroupMember(groupId);
            if (playerRemovedFromGroup)
                MemberRemoved?.Invoke(groupId, player);
        }

        return removed;
    }

    public async Task<bool> RemoveMember(RealmPlayer player, GroupId groupId, CancellationToken cancellationToken = default)
    {
        return await RemoveMember(groupId, player.UserId, cancellationToken);
    }

    public async Task<GroupMemberDto[]> GetGroupMembersByUserId(int userId, int[]? kinds = null, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupMemberData = await _groupRepository.GetGroupMembersByUserId(userId, kinds, cancellationToken);
            return groupMemberData.Select(GroupMemberDto.Map).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> GroupExistsByName(string name, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GroupExistsByName(name, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> GroupExistsByShortcut(string shortcut, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GroupExistsByShortcut(shortcut, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupRoleDto> CreateRole(GroupId groupId, string name, int[] permissions, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupRoleData = await _groupRepository.CreateRole(groupId, name, permissions, cancellationToken);
            return GroupRoleDto.Map(groupRoleData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int[]> GetGroupRoles(GroupId groupId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GetGroupRoles(groupId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int[]> GetRolePermissions(int roleId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GetRolePermissions(roleId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> SetMemberRole(GroupId groupId, int userId, int roleId, CancellationToken cancellationToken = default)
    {
        bool roleChanged = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var roles = await _groupRepository.GetGroupRoles(groupId, cancellationToken);
            if (!roles.Contains(roleId))
                return false;
            
            if (await _groupRepository.SetMemberRole(groupId, userId, roleId, cancellationToken))
            {
                roleChanged = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        if (roleChanged && _usersInUse.TryGetPlayerByUserId(userId, out var player))
        {
            int[] permissions;
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                permissions = await _groupRepository.GetRolePermissions(roleId, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
            var changed = player.Groups.SetGroupRole(groupId, roleId, permissions);
            if (changed)
                MemberChanged?.Invoke(groupId, player);
        }

        return roleChanged;
    }

    public async Task<bool> SetRolePermissions(int roleId, int[] permissions, CancellationToken cancellationToken = default)
    {
        GroupId? groupId = null;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!await _groupRepository.SetRolePermissions(roleId, permissions, cancellationToken))
                return false;
            groupId = await _groupRepository.GetGroupIdByRoleId(roleId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        if(groupId != null)
        {
            foreach (var player in _groupsManager.GetPlayersInGroup(groupId.Value))
            {
                player.Groups.SetGroupRolePermissions(groupId.Value, roleId, permissions);
            }
        }
        return true;
    }
}
