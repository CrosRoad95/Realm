namespace RealmCore.Server.Modules.Server;

public interface IFeedbackService
{
    Task AddOpinion(RealmPlayer player, int opinionId, string opinion, CancellationToken cancellationToken = default);
    Task ChangeLastRating(RealmPlayer player, int ratingId, int rating, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastOpinionDateTime(RealmPlayer player, int opinionId, CancellationToken cancellationToken = default);
    Task<(int, DateTime)?> GetLastRating(RealmPlayer player, int ratingId, CancellationToken cancellationToken = default);
    Task Rate(RealmPlayer player, int ratingId, int rating, CancellationToken cancellationToken = default);
}

internal sealed class PlayerFeedbackService : IFeedbackService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IOpinionRepository _opinionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IServiceScope _serviceScope;

    public PlayerFeedbackService(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _ratingRepository = _serviceScope.ServiceProvider.GetRequiredService<IRatingRepository>();
        _opinionRepository = _serviceScope.ServiceProvider.GetRequiredService<IOpinionRepository>();
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Rate(RealmPlayer player, int ratingId, int rating, CancellationToken cancellationToken = default)
    {
        await _ratingRepository.Rate(player.UserId, ratingId, rating, _dateTimeProvider.Now, cancellationToken);
    }

    public async Task ChangeLastRating(RealmPlayer player, int ratingId, int rating, CancellationToken cancellationToken = default)
    {
        await _ratingRepository.ChangeLastRating(player.UserId, ratingId, rating, _dateTimeProvider.Now, cancellationToken);
    }

    public async Task<(int, DateTime)?> GetLastRating(RealmPlayer player, int ratingId, CancellationToken cancellationToken = default)
    {
        return await _ratingRepository.GetLastRating(player.UserId, ratingId, cancellationToken);
    }

    public async Task AddOpinion(RealmPlayer player, int opinionId, string opinion, CancellationToken cancellationToken = default)
    {
        await _opinionRepository.Add(player.UserId, opinionId, opinion, _dateTimeProvider.Now, cancellationToken);
    }

    public async Task<DateTime?> GetLastOpinionDateTime(RealmPlayer player, int opinionId, CancellationToken cancellationToken = default)
    {
        return await _opinionRepository.GetLastOpinionDateTime(player.UserId, opinionId, cancellationToken);
    }
}
