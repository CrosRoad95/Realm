namespace RealmCore.Persistence.Repository;

public interface IGroupRepository
{
    Task<GroupData> Create(string groupName, string shortcut, byte kind = 1, CancellationToken cancellationToken = default);
    Task<GroupMemberData?> TryAddMember(int groupId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default);
    Task<bool> ExistsByName(string groupName, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default);
    Task<bool> ExistsByShortcut(string shortcut, CancellationToken cancellationToken = default);
    Task<GroupData?> GetByName(string groupName, CancellationToken cancellationToken = default);
    Task<GroupData?> GetGroupByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default);
    Task<bool> IsUserInGroup(int groupId, int userId, CancellationToken cancellationToken = default);
    Task<bool> TryRemoveMember(int groupId, int userId, CancellationToken cancellationToken = default);
}

internal sealed class GroupRepository : IGroupRepository
{
    private readonly IDb _db;

    public GroupRepository(IDb db)
    {
        _db = db;
    }

    public async Task<GroupData?> GetByName(string groupName, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Include(x => x.Members)
            .Where(x => x.Name == groupName);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupData?> GetGroupByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Include(x => x.Members)
            .Where(x => x.Name == groupName || x.Shortcut == shortcut);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByName(string groupName, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Name == groupName);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Name == groupName || x.Shortcut == shortcut);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByShortcut(string shortcut, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Shortcut == shortcut);
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetGroupIdByName(string groupName, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Name == groupName)
            .Select(x => x.Id);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<GroupData> Create(string groupName, string shortcut, byte kind = 1, CancellationToken cancellationToken = default)
    {
        var group = new GroupData
        {
            Name = groupName,
            Shortcut = shortcut,
            Kind = kind,
        };
        _db.Groups.Add(group);
        await _db.SaveChangesAsync(cancellationToken);
        return group;
    }

    public async Task<GroupMemberData?> TryAddMember(int groupId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default)
    {
        var groupMember = new GroupMemberData
        {
            GroupId = groupId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        };
        _db.GroupMembers.Add(groupMember);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return groupMember;
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    public async Task<bool> IsUserInGroup(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> TryRemoveMember(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        return await query.ExecuteDeleteAsync(cancellationToken) == 1;
    }
}
