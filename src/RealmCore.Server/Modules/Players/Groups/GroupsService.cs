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
    private readonly GroupRepository _groupRepository;

    public GroupsService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _dateTimeProvider = dateTimeProvider;
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

    public async Task<GroupDto?> GetGroupByName(string groupName, CancellationToken cancellationToken = default)
    {
        var groupData = await _groupRepository.GetByName(groupName, cancellationToken);
        if (groupData == null)
            return null;

        return GroupDto.Map(groupData);
    }

    public async Task<GroupDto?> GetGroupByNameOrShortCut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        var groupData = await _groupRepository.GetGroupByNameOrShortcut(groupName, shortcut, cancellationToken);
        if (groupData == null)
            return null;

        return GroupDto.Map(groupData);
    }

    public async Task<bool> GroupExistsByNameOrShorCut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        return await _groupRepository.ExistsByNameOrShortcut(groupName, shortcut, cancellationToken);
    }

    public async Task<bool> TryAddMember(RealmPlayer player, int groupId, int? roleId = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        if (player.Groups.IsMember(groupId))
            return false;

        var groupMemberData = await _groupRepository.TryAddMember(groupId, player.UserId, _dateTimeProvider.Now, roleId, metadata, cancellationToken);
        if (groupMemberData == null)
            return false;

        player.Groups.AddGroupMember(groupMemberData);
        return true;
    }

    public async Task<bool> RemoveMember(RealmPlayer player, int groupId, CancellationToken cancellationToken = default)
    {
        if (!player.Groups.IsMember(groupId))
            return false;

        if (await _groupRepository.TryRemoveMember(groupId, player.UserId, cancellationToken))
        {
            player.Groups.RemoveGroupMember(groupId);
        }
        return false;
    }
}
