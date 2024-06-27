using Microsoft.Extensions.DependencyInjection;

namespace RealmCore.Server.Modules.Users;

public interface IPlayerUserService
{
    Task<UserData?> GetUserByUserName(string userName, CancellationToken cancellationToken = default);
    Task<bool> TryUpdateLastNickname(int userId, string nick, CancellationToken cancellationToken = default);
}

internal sealed class PlayersUsersService : IPlayerUserService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayersUsersService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceProvider = serviceProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<UserData?> GetUserByUserName(string userName, CancellationToken cancellationToken = default)
    {
        await using var serviceScope = _serviceProvider.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<IDb>();

        var query = db.Users
            .TagWithSource(nameof(PlayersUsersService))
            .IncludeAll(_dateTimeProvider.Now)
            .Where(u => u.UserName == userName);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TryUpdateLastNickname(int userId, string nick, CancellationToken cancellationToken = default)
    {
        await using var serviceScope = _serviceProvider.CreateAsyncScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<IDb>();

        var query = db.Users
            .TagWithSource(nameof(PlayersUsersService))
            .Where(u => u.Id == userId);

        return await query.ExecuteUpdateAsync(x => x.SetProperty(y => y.Nick, nick), cancellationToken) == 1;
    }
}
