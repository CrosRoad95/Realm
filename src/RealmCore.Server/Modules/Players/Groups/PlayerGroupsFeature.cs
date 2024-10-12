using RealmCore.Persistence.Data;

namespace RealmCore.Server.Modules.Players.Groups;

public sealed class PlayerGroupsFeature : IPlayerFeature, IEnumerable<GroupMemberDto>, IUsesUserPersistentData
{
    private readonly object _lock = new();
    private ICollection<GroupMemberData> _groupMembers = [];
    private HashSet<int> _upgrades = [];

    public event Action? VersionIncreased;

    public RealmPlayer Player { get; init; }

    public event Action<PlayerGroupsFeature, GroupMemberDto>? Added;
    public event Action<PlayerGroupsFeature, GroupMemberDto>? Removed;
    public event Action<PlayerGroupsFeature, int, int?>? GroupRoleChanged;
    public event Action<PlayerGroupsFeature, int>? UpgradeAdded;
    public event Action<PlayerGroupsFeature, int>? UpgradeRemoved;

    public int[] Ids
    {
        get
        {
            lock (_lock)
            {
                return _groupMembers.Select(x => x.Id).ToArray();
            }
        }
    }
    
    public int[] Upgrades
    {
        get
        {
            lock (_lock)
            {
                return [.. _upgrades];
            }
        }
    }

    public PlayerGroupsFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public void LogIn(UserData userData)
    {
        int[] upgrades;
        lock (_lock)
        {
            _groupMembers = [.. userData.GroupMembers];
            foreach (var groupMember in _groupMembers)
            {
                if(groupMember.Group != null)
                {
                    foreach (var upgrade in groupMember.Group.Upgrades)
                    {
                        _upgrades.Add(upgrade.UpgradeId);
                    }
                }
            }
            upgrades = [.. _upgrades];
        }

        foreach (var upgradeId in upgrades)
        {
            UpgradeAdded?.Invoke(this, upgradeId);
        }
    }

    internal bool AddGroupMember(GroupMemberData groupMemberData)
    {
        List<int> addedUpgrades = [];
        lock (_lock)
        {
            var groupMember = _groupMembers.FirstOrDefault(x => x.GroupId == groupMemberData.GroupId);
            if (groupMember != null)
                return false;

            _groupMembers.Add(groupMemberData);
            foreach (var upgrade in groupMemberData!.Group!.Upgrades)
            {
                if (_upgrades.Add(upgrade.UpgradeId))
                {
                    addedUpgrades.Add(upgrade.UpgradeId);
                }
            }
        }
        VersionIncreased?.Invoke();
        Added?.Invoke(this, GroupMemberDto.Map(groupMemberData));
        foreach (var upgradeId in addedUpgrades)
            UpgradeAdded?.Invoke(this, upgradeId);

        return true;
    }

    internal bool RemoveGroupMember(int groupId)
    {
        List<int> removedUpgrades = [];
        GroupMemberData? groupMemberData = null;
        lock (_lock)
        {
            groupMemberData = _groupMembers.FirstOrDefault(x => x.GroupId == groupId);
            if (groupMemberData == null)
                return false;
            _groupMembers.Remove(groupMemberData);

            var upgrades = _groupMembers.SelectMany(x => x.Group!.Upgrades).Select(x => x.UpgradeId);
            foreach (var upgradeId in _upgrades.Except(upgrades))
            {
                if (_upgrades.Remove(upgradeId))
                    removedUpgrades.Add(upgradeId);
            }
        }

        VersionIncreased?.Invoke();

        foreach (var upgradeId in removedUpgrades)
            UpgradeRemoved?.Invoke(this, upgradeId);

        Removed?.Invoke(this, GroupMemberDto.Map(groupMemberData));
        return true;
    }

    internal bool SetGroupRolePermissions(int groupId, int roleId, int[] permissions)
    {
        GroupMemberData? groupMemberData = null;
        lock (_lock)
        {
            groupMemberData = _groupMembers.FirstOrDefault(x => x.GroupId == groupId);
            if (groupMemberData == null || groupMemberData.RoleId != roleId)
                return false;

            groupMemberData.Role = new GroupRoleData
            {
                Permissions = permissions.Select(x => new GroupRolePermissionData
                {
                    PermissionId = x
                }).ToArray()
            };
        }

        VersionIncreased?.Invoke();
        GroupRoleChanged?.Invoke(this, groupId, roleId);
        return true;
    }

    internal bool SetGroupRole(int groupId, int? roleId, int[] permissions)
    {
        GroupMemberData? groupMemberData = null;
        lock (_lock)
        {
            groupMemberData = _groupMembers.FirstOrDefault(x => x.GroupId == groupId);
            if (groupMemberData == null)
                return false;

            groupMemberData.RoleId = roleId;
            if(roleId == null)
            {
                groupMemberData.Role = null;
            }
            else
            {
                groupMemberData.Role = new GroupRoleData
                {
                    Permissions = permissions.Select(x => new GroupRolePermissionData
                    {
                        PermissionId = x
                    }).ToArray()
                };
            }
        }

        VersionIncreased?.Invoke();
        GroupRoleChanged?.Invoke(this, groupId, roleId);
        return true;
    }

    internal bool RemoveFromRole(int groupId, int? roleId = null)
    {
        lock (_lock)
        {
            var groupMemberData = _groupMembers.FirstOrDefault(x => x.GroupId == groupId);
            if (groupMemberData == null)
                return false;

            if (roleId != null && roleId != groupMemberData.RoleId)
                return false;

            groupMemberData.RoleId = null;
            groupMemberData.Role = null;
        }

        VersionIncreased?.Invoke();
        GroupRoleChanged?.Invoke(this, groupId, null);
        return true;

    }

    public bool IsMember(int groupId)
    {
        lock (_lock)
            return _groupMembers.Any(x => x.GroupId == groupId);
    }

    public bool TryGetMember(int groupId, out GroupMemberDto groupMember)
    {
        lock (_lock)
        {
            groupMember = GroupMemberDto.Map(_groupMembers.FirstOrDefault(x => x.GroupId == groupId));
            return groupMember != null;
        }
    }

    public GroupMemberDto? GetById(int groupId)
    {
        GroupMemberData? groupMemberData;
        lock (_lock)
            groupMemberData = _groupMembers.Where(x => x.GroupId == groupId).FirstOrDefault();
        return GroupMemberDto.Map(groupMemberData);
    }

    public bool AddUpgrade(int groupId, int upgradeId)
    {
        bool added = false;
        lock (_lock)
        {
            var groupMemberData = _groupMembers.Where(x => x.GroupId == groupId).FirstOrDefault();
            if (groupMemberData!.Group!.Upgrades.Any(x => x.UpgradeId == upgradeId))
                return false;

            groupMemberData.Group.Upgrades.Add(new GroupUpgradeData
            {
                GroupId = groupId,
                UpgradeId = upgradeId
            });

            added = _upgrades.Add(upgradeId);
        }

        if(added)
            UpgradeAdded?.Invoke(this, upgradeId);

        return added;
    }

    public bool HasUpgrade(int upgradeId)
    {
        lock (_lock)
            return _upgrades.Contains(upgradeId);
    }

    public IEnumerator<GroupMemberDto> GetEnumerator()
    {
        GroupMemberDto[] view;
        lock (_lock)
            view = [.. _groupMembers.Select(GroupMemberDto.Map)];

        foreach (var groupMemberDto in view)
            yield return groupMemberDto;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Clear()
    {
        lock (_lock)
            _groupMembers.Clear();
    }
}
