namespace RealmCore.Server.Modules.Users;

public interface IPlayerUserService
{
    Task<UserData?> GetUserByUserName(string userName, DateTime now, CancellationToken cancellationToken = default);
}

internal sealed class PlayerUserService : IPlayerUserService
{
    private readonly IDb _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayerUserService(IDb db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<UserData?> GetUserByUserName(string userName, DateTime now, CancellationToken cancellationToken = default)
    {
        var query = _db.Users
            .TagWithSource(nameof(PlayerUserService))
            .IncludeAll(_dateTimeProvider.Now)
            .Where(u => u.UserName == userName);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
