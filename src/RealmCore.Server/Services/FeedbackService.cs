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

    public async Task Rate(RealmPlayer player, int ratingId, int rating)
    {
        await _ratingRepository.Rate(player.UserId, ratingId, rating, _dateTimeProvider.Now);
    }
    
    public async Task ChangeLastRating(RealmPlayer player, int ratingId, int rating)
    {
        await _ratingRepository.ChangeLastRating(player.UserId, ratingId, rating, _dateTimeProvider.Now);
    }

    public async Task<(int, DateTime)?> GetLastRating(RealmPlayer player, int ratingId)
    {
        return await _ratingRepository.GetLastRating(player.UserId, ratingId);
    }

    public async Task AddOpinion(RealmPlayer player, int opinionId, string opinion)
    {
        await _opinionRepository.AddOpinion(player.UserId, opinionId, opinion, _dateTimeProvider.Now);
    }

    public async Task<DateTime?> GetLastOpinionDateTime(RealmPlayer player, int opinionId)
    {
        return await _opinionRepository.GetLastOpinionDateTime(player.UserId, opinionId);
    }
}
