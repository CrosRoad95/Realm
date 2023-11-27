namespace RealmCore.Server.Interfaces;

public interface IFeedbackService
{
    Task AddOpinion(RealmPlayer player, int opinionId, string opinion, CancellationToken cancellationToken = default);
    Task ChangeLastRating(RealmPlayer player, int ratingId, int rating, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastOpinionDateTime(RealmPlayer player, int opinionId, CancellationToken cancellationToken = default);
    Task<(int, DateTime)?> GetLastRating(RealmPlayer player, int ratingId, CancellationToken cancellationToken = default);
    Task Rate(RealmPlayer player, int ratingId, int rating, CancellationToken cancellationToken = default);
}
