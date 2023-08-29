using RealmCore.Persistence.Interfaces;

namespace RealmCore.Server.Services;

internal class FeedbackService : IFeedbackService
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

    public async Task Rate(Entity playerEntity, int ratingId, int rating)
    {
        await _ratingRepository.Rate(playerEntity.GetRequiredComponent<UserComponent>().Id, ratingId, rating, _dateTimeProvider.Now);
    }
    
    public async Task ChangeLastRating(Entity playerEntity, int ratingId, int rating)
    {
        await _ratingRepository.ChangeLastRating(playerEntity.GetRequiredComponent<UserComponent>().Id, ratingId, rating, _dateTimeProvider.Now);
    }

    public async Task<(int, DateTime)?> GetLastRating(Entity playerEntity, int ratingId)
    {
        return await _ratingRepository.GetLastRating(playerEntity.GetRequiredComponent<UserComponent>().Id, ratingId);
    }

    public async Task AddOpinion(Entity playerEntity, int opinionId, string opinion)
    {
        await _opinionRepository.AddOpinion(playerEntity.GetRequiredComponent<UserComponent>().Id, opinionId, opinion, _dateTimeProvider.Now);
    }

    public async Task<DateTime?> GetLastOpinionDateTime(Entity playerEntity, int opinionId)
    {
        return await _opinionRepository.GetLastOpinionDateTime(playerEntity.GetRequiredComponent<UserComponent>().Id, opinionId);
    }
}
