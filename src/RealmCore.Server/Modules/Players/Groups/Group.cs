namespace RealmCore.Server.Modules.Players.Groups;

public sealed class GroupMemberDto : IEqualityComparer<GroupMemberDto>
{
    public required int Id { get; init; }
    public required int GroupId { get; init; }
    public required int? RoleId { get; init; }
    public required string? Metadata { get; init; }
    public required DateTime? CreatedAt { get; init; }
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
            Metadata = groupMemberData.Metadata,
            CreatedAt = groupMemberData.CreatedAt,
        };
    }
}

public sealed class GroupDto : IEqualityComparer<GroupDto>
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string? Shortcut { get; init; }
    public required byte? Kind { get; init; }
    public required DateTime? CreatedAt { get; init; }
    public bool Equals(GroupDto? x, GroupDto? y) => x?.Id == y?.Id;

    public int GetHashCode([DisallowNull] GroupDto obj) => obj.Id;

    [return: NotNullIfNotNull(nameof(groupData))]
    public static GroupDto? Map(GroupData? groupData)
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
        };
    }
}
