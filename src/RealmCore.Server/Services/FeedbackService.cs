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

    public async Task<bool> Rate(Entity playerEntity, int ratingId, int rating)
    {
        if(playerEntity.TryGetComponent(out UserComponent userComponent))
        {
            return await _ratingRepository.Rate(userComponent.Id, ratingId, rating, _dateTimeProvider.Now);
        }
        return false;
    }
    
    public async Task ChangeLastRating(Entity playerEntity, int ratingId, int rating)
    {
        if (playerEntity.TryGetComponent(out UserComponent userComponent))
        {
            await _ratingRepository.ChangeLastRating(userComponent.Id, ratingId, rating, _dateTimeProvider.Now);
        }
    }

    public async Task<(int, DateTime)?> GetLastRating(Entity playerEntity, int ratingId)
    {
        if (playerEntity.TryGetComponent(out UserComponent userComponent))
        {
            return await _ratingRepository.GetLastRating(userComponent.Id, ratingId);
        }
        return null;
    }

    public async Task<bool> AddOpinion(Entity playerEntity, int opinionId, string opinion)
    {
        if (playerEntity.TryGetComponent(out UserComponent userComponent))
        {
            return await _opinionRepository.AddOpinion(userComponent.Id, opinionId, opinion, _dateTimeProvider.Now);
        }
        return false;
    }

    public async Task<DateTime?> GetLastOpinionDateTime(Entity playerEntity, int opinionId)
    {
        if (playerEntity.TryGetComponent(out UserComponent userComponent))
        {
            return await _opinionRepository.GetLastOpinionDateTime(userComponent.Id, opinionId);
        }

        return null;
    }
}
