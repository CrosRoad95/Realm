namespace RealmCore.Server.Modules.Users;

public sealed class PlayersUsersService
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UsersRepository _usersRepository;

    public PlayersUsersService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceProvider = serviceProvider;
        _dateTimeProvider = dateTimeProvider;
        _serviceScope = _serviceProvider.CreateAsyncScope();
        _usersRepository = _serviceScope.ServiceProvider.GetRequiredService<UsersRepository>();
    }

    public async Task<UserData?> GetUserByUserName(string userName, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.GetUserByUserName(userName, _dateTimeProvider.Now, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> TryUpdateLastNickname(int userId, string nick, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.TryUpdateLastNickname(userId, nick, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
