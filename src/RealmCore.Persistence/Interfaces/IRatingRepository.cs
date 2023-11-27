namespace RealmCore.Persistence.Interfaces;

public interface IRatingRepository
{
    Task ChangeLastRating(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default);
    Task<(int, DateTime)?> GetLastRating(int userId, int ratingId, CancellationToken cancellationToken = default);
    Task<bool> Rate(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default);
}