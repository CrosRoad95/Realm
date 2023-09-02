namespace RealmCore.Server.Interfaces;

public interface IFeedbackService
{
    Task<bool> AddOpinion(Entity playerEntity, int opinionId, string opinion);
    Task ChangeLastRating(Entity playerEntity, int ratingId, int rating);
    Task<DateTime?> GetLastOpinionDateTime(Entity playerEntity, int opinionId);
    Task<(int, DateTime)?> GetLastRating(Entity playerEntity, int ratingId);
    Task<bool> Rate(Entity playerEntity, int ratingId, int rating);
}
