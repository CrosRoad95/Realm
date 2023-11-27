namespace RealmCore.Server.Services;

internal sealed class FeedbackService : IFeedbackService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IOpinionRepository _opinionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public FeedbackService(IRatingRepository ratingRepository, IOpinionRepository opinionRepository, IDateTimeProvider dateTimeProvider)
    {
        _ratingRepository = ratingRepository;
        _opinionRepository = opinionRepository;
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
        await _opinionRepository.AddOpinion(player.UserId, opinionId, opinion, _dateTimeProvider.Now, cancellationToken);
    }

    public async Task<DateTime?> GetLastOpinionDateTime(RealmPlayer player, int opinionId, CancellationToken cancellationToken = default)
    {
        return await _opinionRepository.GetLastOpinionDateTime(player.UserId, opinionId, cancellationToken);
    }
}
