namespace RealmCore.Server.Modules.Players.Groups;

public record GroupMemberStatistic(DateOnly Date, int StatisticId, float Value);

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

    public event Action<GroupId, int, RealmPlayer?>? MemberAdded;
    public event Action<GroupId, int, RealmPlayer?>? MemberRemoved;
    public event Action<GroupId, int, RealmPlayer?>? MemberChanged;

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

    public async Task<GroupDto?> Create(string name, string? shortcut = null, int kind = 0, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupData = await _transactionContext.ExecuteAsync(async () =>
            {
                if (shortcut != null && await _groupRepository.GroupExistsByNameOrShortcut(name, shortcut, cancellationToken))
                    return null;

                var groupData = await _groupRepository.Create(name, _dateTimeProvider.Now, shortcut, (byte)kind, cancellationToken);
                return groupData;
            }, cancellationToken);

            if(groupData == null)
            {
                ;
            }
            return GroupDto.Map(groupData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupDto?> GetGroupById(GroupId id, CancellationToken cancellationToken = default)
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

    public async Task<bool> AddMember(RealmPlayer player, GroupId groupId, int? roleId = null, string? metadata = null, bool force = false, CancellationToken cancellationToken = default)
    {
        return await AddMember(player.UserId, groupId, roleId, metadata, force, cancellationToken);
    }

    public async Task<bool> AddMember(int userId, GroupId groupId, int? roleId = null, string? metadata = null, bool force = false, CancellationToken cancellationToken = default)
    {
        bool added = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            added = await _groupRepository.AddMember(groupId, userId, _dateTimeProvider.Now, roleId, metadata, force, cancellationToken);

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

        if (added)
        {
            MemberAdded?.Invoke(groupId, userId, null);
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
                MemberRemoved?.Invoke(groupId, userId, player);
        }

        return removed;
    }

    public async Task<bool> RemoveMember(RealmPlayer player, GroupId groupId, CancellationToken cancellationToken = default)
    {
        return await RemoveMember(player.UserId, groupId, cancellationToken);
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

    public async Task<int[]> GetGroupRolesIds(GroupId groupId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GetGroupRolesIds(groupId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<GroupRoleDto[]> GetGroupRoles(GroupId groupId, CancellationToken cancellationToken = default)
    {
        GroupRoleData[] groupRoles;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            groupRoles = await _groupRepository.GetGroupRoles(groupId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return groupRoles.Select(GroupRoleDto.Map).ToArray();
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

    public async Task<bool> SetMemberRole(GroupId groupId, int userId, GroupRoleId? roleId, CancellationToken cancellationToken = default)
    {
        bool roleChanged = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if(roleId != null)
            {
                var roles = await _groupRepository.GetGroupRolesIds(groupId, cancellationToken);
                if (!roles.Contains(roleId.Value))
                    return false;
            }
            
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
                if(roleId == null)
                {
                    permissions = [];
                }
                else
                {
                    permissions = await _groupRepository.GetRolePermissions(roleId.Value, cancellationToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
            var changed = player.Groups.SetGroupRole(groupId, roleId, permissions);
            if (changed)
                MemberChanged?.Invoke(groupId, userId, player);
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

    public async Task<bool> SetGroupSetting(GroupId groupId, int settingId, string value, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.SetGroupSetting(groupId, settingId, value, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<string?> GetGroupSetting(GroupId groupId, int settingId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GetGroupSetting(groupId, settingId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<string?> GetGroupName(GroupId groupId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GetGroupName(groupId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IReadOnlyDictionary<int, string>> GetGroupSettings(GroupId groupId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GetGroupSettings(groupId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoveAllJoinRequestsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.RemoveAllJoinRequestsByUserId(userId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
    public async Task<bool> CreateJoinRequest(GroupId groupId, RealmPlayer player, string? metadata = null, CancellationToken cancellationToken = default)
    {
        return await CreateJoinRequest(groupId, player.UserId, metadata, cancellationToken);
    }

    public async Task<bool> CreateJoinRequest(GroupId groupId, int userId, string? metadata = null, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.CreateJoinRequest(groupId, userId, _dateTimeProvider.Now, metadata, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupJoinRequestDto[]> GetJoinRequestsByUserId(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        return await GetJoinRequestsByUserId(player.UserId, cancellationToken);
    }

    public async Task<GroupJoinRequestDto[]> GetJoinRequestsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var requests = await _groupRepository.GetJoinRequestsByUserId(userId, cancellationToken);
            return requests.Select(GroupJoinRequestDto.Map).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<GroupJoinRequestDto[]> GetJoinRequestsByGroupId(GroupId groupId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var requests = await _groupRepository.GetJoinRequestsByGroupId(groupId, cancellationToken);
            return requests.Select(GroupJoinRequestDto.Map).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoveJoinRequest(GroupId groupId, RealmPlayer player, CancellationToken cancellationToken = default)
    {
        return await RemoveJoinRequest(groupId, player.UserId, cancellationToken);
    }

    public async Task<bool> RemoveJoinRequest(GroupId groupId, int userId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.RemoveJoinRequest(groupId, userId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int> CountJoinRequestsByUserId(int userId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.CountJoinRequestsByUserId(userId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int> CountJoinRequestsByGroupId(GroupId groupId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.CountJoinRequestsByGroupId(groupId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupMemberDto[]> GetGroupMembers(GroupId groupId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var members = await _groupRepository.GetGroupMembers(groupId, cancellationToken);
            return members.Select(GroupMemberDto.Map).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoveRole(GroupRoleId groupRoleId, CancellationToken cancellationToken = default)
    {
        GroupId? groupId = null;
        bool removed = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            await _transactionContext.ExecuteAsync(async () =>
            {
                groupId = await _groupRepository.GetGroupIdByRoleId(groupRoleId, cancellationToken);
                await _groupRepository.RemoveAllMembersFromRole(groupRoleId, cancellationToken);
                removed = await _groupRepository.RemoveRole(groupRoleId, cancellationToken);
            }, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        if (removed && groupId != null)
        {
            foreach (var player in _groupsManager.GetPlayersInGroup(groupId.Value))
            {
                player.Groups.RemoveFromRole(groupId.Value, groupRoleId);
            }
        }
        return removed;
    }

    public async Task<int[]> GetGroupMemberPermissions(GroupId groupId, int userId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var permissions = await _groupRepository.GetGroupMemberPermissions(groupId, userId, cancellationToken);
            return permissions;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<string?> GetRoleName(GroupRoleId groupRoleId, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GetRoleName(groupRoleId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupDto[]> GetAll(int page, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        GroupData[] groups;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            groups = await _groupRepository.GetAll(page, pageSize, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return groups.Select(GroupDto.Map).ToArray();
    }

    public async Task<bool> GiveMoney(int groupId, decimal amount, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.GiveMoney(groupId, amount, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TakeMoney(GroupId groupId, decimal amount, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.TakeMoney(groupId, amount, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> AddUpgrade(GroupId groupId, int upgradeId, CancellationToken cancellationToken = default)
    {
        bool added = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            added = await _groupRepository.AddUpgrade(groupId, upgradeId, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        if (added)
        {
            foreach (var player in _groupsManager.GetPlayersInGroup(groupId.id))
            {
                player.Groups.AddUpgrade(groupId.id, upgradeId);
            }

        }

        return added;
    }

    public async Task<bool> IncreaseStatistic(GroupId groupId, int userId, int statisticId, DateOnly date, float value, CancellationToken cancellationToken = default)
    {
        if (value < 0)
            return false;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.IncreaseStatistic(groupId, userId, statisticId, date, value, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupMemberStatistic[]> GetStatistics(GroupId groupId, int userId, DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        GroupMemberStatisticData[] statistics;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            statistics = await _groupRepository.GetStatistics(groupId, userId, date, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return statistics.Select(x => new GroupMemberStatistic(x.Date, x.StatisticId, x.Value)).ToArray();
    }

    public async Task<GroupMemberStatistic[]> GetStatistics(GroupId groupId, int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        GroupMemberStatisticData[] statistics;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            statistics = await _groupRepository.GetStatistics(groupId, userId, from, to, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return statistics.Select(x => new GroupMemberStatistic(x.Date, x.StatisticId, x.Value)).ToArray();
    }
}
