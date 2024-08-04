namespace RealmCore.Server.Modules.Players.Groups;

public sealed class GroupsService
{
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

    public async Task<GroupDto> CreateGroup(string groupName, string shortcut, int groupKind = 0, CancellationToken cancellationToken = default)
    {
        if (await _groupRepository.ExistsByName(groupName, cancellationToken))
            throw new GroupNameInUseException(groupName);

        if (await _groupRepository.ExistsByShortcut(shortcut, cancellationToken))
            throw new GroupShortcutInUseException(shortcut);

        var groupData = await _groupRepository.Create(groupName, shortcut, (byte)groupKind, cancellationToken);
        return GroupDto.Map(groupData);
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
