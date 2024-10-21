namespace RealmCore.Server.Modules.Players.Groups;

public sealed class GroupRoleDto : IEqualityComparer<GroupRoleData>
{
    private GroupRoleData? Data { get; init; }
    public required GroupRoleId Id { get; init; }
    public required GroupId GroupId { get; init; }
    public required string Name { get; init; }
    public required int[] Permissions { get; init; }
    public required int MembersCount { get; init; }
    public GroupMemberDto[] GetMembers() => Data != null ? Data.Members.Select(GroupMemberDto.Map).ToArray() : [];
    public bool Equals(GroupRoleData? x, GroupRoleData? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] GroupRoleData obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(groupRoleData))]
    public static GroupRoleDto? Map(GroupRoleData? groupRoleData)
    {
        if (groupRoleData == null)
            return null;

        return new()
        {
            Data = groupRoleData,
            Id = groupRoleData.Id,
            GroupId = groupRoleData.GroupId,
            Name = groupRoleData.Name,
            MembersCount = groupRoleData.Members.Count,
            Permissions = groupRoleData.Permissions.Select(x => x.PermissionId).ToArray()
        };
    }
}

public sealed class GroupMemberDto : IEqualityComparer<GroupMemberDto>
{
    public required GroupMemberId Id { get; init; }
    public required GroupId GroupId { get; init; }
    public required int UserId { get; init; }
    public required GroupRoleId? RoleId { get; init; }
    public required int[] Permissions { get; init; } = [];
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
            UserId = groupMemberData.UserId,
            RoleId = groupMemberData.RoleId,
            Permissions = groupMemberData.Role != null ? groupMemberData.Role.Permissions.Select(x => x.PermissionId).ToArray() : [],
            Metadata = groupMemberData.Metadata,
            CreatedAt = groupMemberData.CreatedAt,
            Group = GroupDto.Map(groupMemberData.Group)
        };
    }
}

public sealed class GroupDto : IEqualityComparer<GroupDto>
{
    private GroupData? Data { get; init; }
    public required GroupId Id { get; init; }
    public required string Name { get; init; }
    public required string? Shortcut { get; init; }
    public required byte? Kind { get; init; }
    public required DateTime? CreatedAt { get; init; }
    public required GroupRoleDto[] Roles { get; init; }
    public required IReadOnlyDictionary<int, string> Settings { get; init; }
    public IEnumerable<GroupMemberDto> Members => Data != null ? Data.Members.Select(GroupMemberDto.Map) : Enumerable.Empty<GroupMemberDto>();
    public required decimal Money { get; init; }

    public bool Equals(GroupDto? x, GroupDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] GroupDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(groupData))]
    public static GroupDto? Map(GroupData? groupData)
    {
        if (groupData == null)
            return null;

        return new()
        {
            Data = groupData,
            Id = groupData.Id,
            Name = groupData.Name,
            Shortcut = groupData.Shortcut,
            Kind = groupData.Kind,
            CreatedAt = groupData.CreatedAt,
            Money = groupData.Money,
            Roles = groupData.Roles.Select(GroupRoleDto.Map).ToArray(),
            Settings = groupData.Settings.ToDictionary(x => x.SettingId, x => x.Value),
        };
    }
}

public sealed class GroupJoinRequestDto : IEqualityComparer<GroupJoinRequestDto>
{
    public required GroupId GroupId { get; init; }
    public required int UserId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string Metadata { get; init; }
    public required GroupDto Group { get; init; }
    public bool Equals(GroupJoinRequestDto? x, GroupJoinRequestDto? y) => x?.GroupId == y?.GroupId;

    public int GetHashCode([DisallowNull] GroupJoinRequestDto obj) => obj.GroupId;

    [return: NotNullIfNotNull(nameof(groupJoinRequestData))]
    public static GroupJoinRequestDto? Map(GroupJoinRequestData? groupJoinRequestData)
    {
        if (groupJoinRequestData == null)
            return null;

        return new()
        {
            GroupId = groupJoinRequestData.GroupId,
            UserId = groupJoinRequestData.UserId,
            CreatedAt = groupJoinRequestData.CreatedAt,
            Metadata = groupJoinRequestData.Metadata,
            Group = GroupDto.Map(groupJoinRequestData.Group)
        };
    }
}
