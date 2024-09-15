namespace RealmCore.Persistence.Data;

public sealed class GroupData
{
    public int Id { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string? Shortcut { get; set; }
    public byte? Kind { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<GroupMemberData> Members { get; set; } = [];
    public ICollection<GroupRoleData> Roles { get; set; } = [];
    public ICollection<GroupEventData> Events { get; set; } = [];
    public ICollection<GroupSettingData> Settings { get; set; } = [];
    public ICollection<GroupJoinRequestData> JoinRequests { get; set; } = [];
    public ICollection<VehicleGroupAccessData> VehicleAccesses { get; set; } = [];
}

public sealed class GroupMemberData
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public int? RoleId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserData? User { get; set; }
    public GroupData? Group { get; set; }
    public GroupRoleData? Role { get; set; }
}

public sealed class GroupRoleData
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Name { get; set; }

    public GroupData? Group { get; set; }
    public ICollection<GroupRolePermissionData> Permissions { get; set; } = [];
    public ICollection<GroupMemberData> Members { get; set; } = [];
}

public sealed class GroupRolePermissionData
{
    public int GroupRoleId { get; set; }
    public int PermissionId { get; set; }

    public GroupRoleData? GroupRole { get; set; }
}

public sealed class GroupEventData : EventDataBase
{
    public int GroupId { get; set; }
}

public sealed class GroupSettingData
{
    public int GroupId { get; set; }
    public int SettingId { get; set; }
    public string Value { get; set; }
}

public sealed class GroupJoinRequestData
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Metadata { get; set; }

    public GroupData? Group { get; set; }
}
