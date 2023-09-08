namespace RealmCore.Persistence.Repository;

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

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<GroupData?> GetGroupByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Include(x => x.Members)
            .Where(x => x.Name == groupName || x.Shortcut == shortcut);

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByName(string groupName, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Name == groupName);
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByNameOrShortcut(string groupName, string shortcut, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Name == groupName || x.Shortcut == shortcut);
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsByShortcut(string shortcut, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .AsNoTrackingWithIdentityResolution()
            .Where(x => x.Shortcut == shortcut);
        
        return await query.AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> GetGroupIdByName(string groupName, CancellationToken cancellationToken = default)
    {
        var query = _db.Groups
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.Name == groupName)
            .Select(x => x.Id);
        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
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
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return group;
    }

    public async Task<GroupMemberData> AddMember(int groupId, int userId, int rank = 1, string rankName = "", CancellationToken cancellationToken = default)
    {
        var groupMember = new GroupMemberData
        {
            GroupId = groupId,
            UserId = userId,
            Rank = rank,
            RankName = rankName,
        };
        _db.GroupMembers.Add(groupMember);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return groupMember;
    }
    
    public async Task<bool> IsUserInGroup(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);
        return await query.AnyAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> RemoveMember(int groupId, int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.GroupMembers
            .TagWithSource(nameof(GroupRepository))
            .Where(x => x.GroupId == groupId && x.UserId == userId);

        return await query.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false) == 1;
    }
}
