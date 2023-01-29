using Realm.Persistance.Exceptions;

namespace Realm.Persistance.Repository;

internal class GroupRepository : IGroupRepository
{
    private readonly IDb _db;

    public GroupRepository(IDb db)
    {
        _db = db;
    }

    public Task<Group?> GetGroupByName(string groupName) => _db.Groups
        .Include(x => x.Members)
        .Where(x => x.Name == groupName)
        .FirstOrDefaultAsync();
    
    public Task<int> GetGroupIdByName(string groupName) => _db.Groups
        .Where(x => x.Name == groupName)
        .Select(x => x.Id)
        .FirstOrDefaultAsync();

    public async Task<Group> CreateNewGroup(string groupName, string shortcut, byte kind = 1)
    {
        var group = new Group
        {
            Name = groupName,
            Shortcut = shortcut,
            Kind = kind,
        };
        _db.Groups.Add(group);
        await _db.SaveChangesAsync();
        return group;
    }

    public async Task<GroupMember> CreateNewGroupMember(int groupId, Guid userId, int rank = 1, string rankName = "")
    {
        var groupMember = new GroupMember
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

    public async Task<GroupMember> CreateNewGroupMember(string groupName, Guid userId, int rank = 1, string rankName = "")
    {
        var groupId = await GetGroupIdByName(groupName);
        if (groupId == 0)
            throw new GroupNotFoundException(groupName);

        return await CreateNewGroupMember(groupId, userId, rank, rankName);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
