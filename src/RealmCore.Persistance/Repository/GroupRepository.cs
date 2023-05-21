using RealmCore.Persistance.Exceptions;

namespace RealmCore.Persistance.Repository;

internal class GroupRepository : IGroupRepository
{
    private readonly IDb _db;

    public GroupRepository(IDb db)
    {
        _db = db;
    }

    public Task<GroupData?> GetGroupByName(string groupName) => _db.Groups
        .TagWithSource(nameof(GroupRepository))
        .Include(x => x.Members)
        .Where(x => x.Name == groupName)
        .FirstOrDefaultAsync();

    public Task<GroupData?> GetGroupByNameOrShortcut(string groupName, string shortcut) => _db.Groups
        .TagWithSource(nameof(GroupRepository))
        .Include(x => x.Members)
        .Where(x => x.Name == groupName || x.Shortcut == shortcut)
        .FirstOrDefaultAsync();

    public Task<bool> ExistsByName(string groupName) => _db.Groups
        .TagWithSource(nameof(GroupRepository))
        .AsNoTrackingWithIdentityResolution()
        .Where(x => x.Name == groupName)
        .AnyAsync();

    public Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut) => _db.Groups
        .TagWithSource(nameof(GroupRepository))
        .AsNoTrackingWithIdentityResolution()
        .Where(x => x.Name == groupName || x.Shortcut == shortcut)
        .AnyAsync();

    public Task<bool> ExistsByShortcut(string shortcut) => _db.Groups
        .TagWithSource(nameof(GroupRepository))
        .AsNoTrackingWithIdentityResolution()
        .Where(x => x.Shortcut == shortcut)
        .AnyAsync();

    public Task<int> GetGroupIdByName(string groupName) => _db.Groups
        .TagWithSource(nameof(GroupRepository))
        .Where(x => x.Name == groupName)
        .Select(x => x.Id)
        .FirstOrDefaultAsync();

    public async Task<GroupData> CreateNewGroup(string groupName, string shortcut, byte kind = 1)
    {
        var group = new GroupData
        {
            Name = groupName,
            Shortcut = shortcut,
            Kind = kind,
        };
        _db.Groups.Add(group);
        await _db.SaveChangesAsync();
        return group;
    }

    public async Task<GroupMemberData> CreateNewGroupMember(int groupId, int userId, int rank = 1, string rankName = "")
    {
        var groupMember = new GroupMemberData
        {
            GroupId = groupId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        };
        _db.GroupMembers.Add(groupMember);
        await _db.SaveChangesAsync();
        return groupMember;
    }

    public async Task<GroupMemberData> CreateNewGroupMember(string groupName, int userId, int rank = 1, string rankName = "")
    {
        var groupId = await GetGroupIdByName(groupName);
        if (groupId == 0)
            throw new GroupNotFoundException(groupName);

        return await CreateNewGroupMember(groupId, userId, rank, rankName);
    }

    public async Task<bool> RemoveGroupMember(int groupId, int userId)
    {
        var member = await _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId)
            .FirstOrDefaultAsync();
        if (member == null)
            return false;
        _db.GroupMembers.Remove(member);
        return await _db.SaveChangesAsync() == 1;
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public Task<int> Commit()
    {
        return _db.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Commit();
        Dispose();
    }
}
