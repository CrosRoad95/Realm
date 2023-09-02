namespace RealmCore.Persistence.Interfaces;

public interface IRatingRepository
{
    Task<bool> ChangeLastRating(int userId, int ratingId, int rating, DateTime dateTime);
    Task<(int, DateTime)?> GetLastRating(int userId, int ratingId);
    Task<bool> Rate(int userId, int ratingId, int rating, DateTime dateTime);
}
