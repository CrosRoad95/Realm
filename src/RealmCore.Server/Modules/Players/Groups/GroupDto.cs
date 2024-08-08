namespace RealmCore.Server.Modules.Players.Groups;

public sealed class GroupRoleDto : IEqualityComparer<GroupRoleData>
{
    public required GroupRoleId Id { get; init; }
    public required GroupId GroupId { get; init; }
    public required string Name { get; init; }
    public required int[] Permissions { get; init; }
    public bool Equals(GroupRoleData? x, GroupRoleData? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] GroupRoleData obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(groupRoleData))]
    public static GroupRoleDto? Map(GroupRoleData? groupRoleData)
    {
        if (groupRoleData == null)
            return null;

        return new()
        {
            Id = groupRoleData.Id,
            GroupId = groupRoleData.GroupId,
            Name = groupRoleData.Name,
            Permissions = groupRoleData.Permissions.Select(x => x.PermissionId).ToArray()
        };
    }
}

public sealed class GroupMemberDto : IEqualityComparer<GroupMemberDto>
{
    public required GroupMemberId Id { get; init; }
    public required GroupId GroupId { get; init; }
    public required GroupRoleId? RoleId { get; init; }
    public required int[]? Permissions { get; init; }
    public required string? Metadata { get; init; }
    public required DateTime? CreatedAt { get; init; }
    public required GroupDto? Group { get; init; }
    public bool Equals(GroupMemberDto? x, GroupMemberDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] GroupMemberDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(groupMemberData))]
    public static GroupMemberDto? Map(GroupMemberData? groupMemberData)
    {
        if (groupMemberData == null)
            return null;

        return new()
        {
            Id = groupMemberData.Id,
            GroupId = groupMemberData.GroupId,
            RoleId = groupMemberData.RoleId,
            Permissions = groupMemberData.Role != null ? groupMemberData.Role.Permissions.Select(x => x.PermissionId).ToArray() : null,
            Metadata = groupMemberData.Metadata,
            CreatedAt = groupMemberData.CreatedAt,
            Group = GroupDto.Map(groupMemberData.Group, false)
        };
    }
}

public sealed class GroupDto : IEqualityComparer<GroupDto>
{
    public required GroupId Id { get; init; }
    public required string Name { get; init; }
    public required string? Shortcut { get; init; }
    public required byte? Kind { get; init; }
    public required DateTime? CreatedAt { get; init; }
    public required GroupMemberDto[] Members { get; init; }
    public required GroupRoleDto[] Roles { get; init; }
    public bool Equals(GroupDto? x, GroupDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] GroupDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(groupData))]
    public static GroupDto? Map(GroupData? groupData, bool mapMembers = true)
    {
        if (groupData == null)
            return null;

        return new()
        {
            Id = groupData.Id,
            Name = groupData.Name,
            Shortcut = groupData.Shortcut,
            Kind = groupData.Kind,
            CreatedAt = groupData.CreatedAt,
            Members = mapMembers ? groupData.Members.Select(GroupMemberDto.Map).ToArray() : [],
            Roles = groupData.Roles.Select(GroupRoleDto.Map).ToArray()
        };
    }
}
