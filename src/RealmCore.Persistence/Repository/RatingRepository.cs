namespace RealmCore.Persistence.Repository;

public interface IRatingRepository
{
    Task ChangeLastRating(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default);
    Task<(int, DateTime)?> GetLastRating(int userId, int ratingId, CancellationToken cancellationToken = default);
    Task<bool> Rate(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default);
}

internal sealed class RatingRepository : IRatingRepository
{
    private readonly IDb _db;

    public RatingRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> Rate(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(Rate));

        _db.Ratings.Add(new RatingData
        {
            UserId = userId,
            RatingId = ratingId,
            Rating = rating,
            DateTime = dateTime
        });

        return await _db.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task ChangeLastRating(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(ChangeLastRating));

        var query = _db.Ratings
            .Where(x => x.UserId == userId && x.RatingId == ratingId)
            .OrderByDescending(x => x.DateTime);
        var ratingData = await query.FirstOrDefaultAsync(cancellationToken);
        if (ratingData != null)
        {
            ratingData.Rating = rating;
            _db.Ratings.Update(ratingData);
        }
        else
        {
            _db.Ratings.Add(new RatingData
            {
                UserId = userId,
                RatingId = ratingId,
                Rating = rating,
                DateTime = dateTime
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(int, DateTime)?> GetLastRating(int userId, int ratingId, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity(nameof(GetLastRating));

        var query = _db.Ratings
            .Where(x => x.UserId == userId && x.RatingId == ratingId)
            .OrderByDescending(x => x.DateTime);
        var ratingData = await query.FirstOrDefaultAsync(cancellationToken);

        if (ratingData == null)
            return null;

        return (ratingData.Rating, ratingData.DateTime);
    }

    public static readonly ActivitySource Activity = new("RealmCore.RatingRepository", "1.0.0");
}
