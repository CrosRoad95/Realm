namespace RealmCore.Server.Interfaces;

public interface IFeedbackService
{
    Task<bool> AddOpinion(RealmPlayer player, int opinionId, string opinion);
    Task ChangeLastRating(RealmPlayer player, int ratingId, int rating);
    Task<DateTime?> GetLastOpinionDateTime(RealmPlayer player, int opinionId);
    Task<(int, DateTime)?> GetLastRating(RealmPlayer player, int ratingId);
    Task<bool> Rate(RealmPlayer player, int ratingId, int rating);
}
