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
    private readonly GroupRepository _groupRepository;

    public event Action<int, RealmPlayer>? MemberAdded;
    public event Action<int, RealmPlayer>? MemberRemoved;

    public GroupsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider, UsersInUse usersInUse)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dateTimeProvider = dateTimeProvider;
        _usersInUse = usersInUse;
        _groupRepository = _serviceProvider.GetRequiredService<GroupRepository>();
    }

    public async Task<OneOf<Created, NameInUse, ShortcutInUse>> Create(string name, string? shortcut = null, int kind = 0, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (await _groupRepository.ExistsByName(name, cancellationToken))
                return new NameInUse();

            if (shortcut != null && await _groupRepository.ExistsByShortcut(shortcut, cancellationToken))
                return new ShortcutInUse();

            var groupData = await _groupRepository.Create(name, shortcut, (byte)kind, cancellationToken);
            return new Created(GroupDto.Map(groupData));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupDto[]> Search(string name, int limit = 10, byte[]? kinds = null, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var results = await _groupRepository.Search(name, limit, kinds, cancellationToken);

            return results.Select(GroupDto.Map).ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public async Task<GroupDto?> GetGroupByName(string groupName, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupData = await _groupRepository.GetByName(groupName, cancellationToken);
            if (groupData == null)
                return null;
            return GroupDto.Map(groupData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<GroupDto?> GetGroupByNameOrShortCut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupData = await _groupRepository.GetGroupByNameOrShortcut(groupName, shortcut, cancellationToken);
            if (groupData == null)
                return null;

            return GroupDto.Map(groupData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> GroupExistsByNameOrShorCut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _groupRepository.ExistsByNameOrShortcut(groupName, shortcut, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryAddMember(RealmPlayer player, int groupId, int? roleId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        bool added = false;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (player.Groups.IsMember(groupId))
                return false;

            var groupMemberData = await _groupRepository.TryAddMember(groupId, player.UserId, _dateTimeProvider.Now, roleId, metadata, cancellationToken);
            if (groupMemberData == null)
                return false;

            added = player.Groups.AddGroupMember(groupMemberData);
        }
        finally
        {
            _semaphore.Release();
        }

        if (added)
        {
            MemberAdded?.Invoke(groupId, player);
        }

        return added;
    }
    
    public async Task<bool> TryAddMember(int userId, int groupId, int? roleId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        if(_usersInUse.TryGetPlayerByUserId(userId, out var player))
        {
            return await TryAddMember(player, groupId, roleId, metadata, cancellationToken);
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var groupMemberData = await _groupRepository.TryAddMember(groupId, userId, _dateTimeProvider.Now, roleId, metadata, cancellationToken);
            return groupMemberData != null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoveMember(RealmPlayer player, int groupId, CancellationToken cancellationToken = default)
    {
        bool removed = false;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!player.Groups.IsMember(groupId))
                return false;

            if (await _groupRepository.TryRemoveMember(groupId, player.UserId, cancellationToken))
                removed = player.Groups.RemoveGroupMember(groupId);
        }
        finally
        {
            _semaphore.Release();
        }

        if (removed)
        {
            MemberRemoved?.Invoke(groupId, player);
        }

        return removed;
    }

    public async Task<bool> RemoveMember(int userId, int groupId, CancellationToken cancellationToken = default)
    {
        if (_usersInUse.TryGetPlayerByUserId(userId, out var player))
        {
            return await RemoveMember(player, groupId, cancellationToken);
        }

        bool removed = false;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (await _groupRepository.TryRemoveMember(groupId, userId, cancellationToken))
                removed = player.Groups.RemoveGroupMember(groupId);
        }
        finally
        {
            _semaphore.Release();
        }

        return removed;
    }
}
