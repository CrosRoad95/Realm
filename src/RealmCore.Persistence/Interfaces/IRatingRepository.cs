namespace RealmCore.Persistence.Interfaces;

public interface IRatingRepository
{
    Task ChangeLastRating(int userId, int ratingId, int rating, DateTime dateTime);
    Task<(int, DateTime)?> GetLastRating(int userId, int ratingId);
    Task Rate(int userId, int id, int rating, DateTime dateTime);
}
