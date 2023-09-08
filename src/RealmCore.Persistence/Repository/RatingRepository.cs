namespace RealmCore.Persistence.Repository;

internal sealed class RatingRepository : IRatingRepository
{
    private readonly IDb _db;

    public RatingRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> Rate(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        _db.Ratings.Add(new RatingData
        {
            UserId = userId,
            RatingId = ratingId,
            Rating = rating,
            DateTime = dateTime
        });

        return await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<bool> ChangeLastRating(int userId, int ratingId, int rating, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        // TODO: Use transaction
        var query = _db.Ratings
            .Where(x => x.UserId == userId && x.RatingId == ratingId)
            .OrderByDescending(x => x.DateTime);
        var ratingData = await query.FirstOrDefaultAsync();
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

        return await _db.SaveChangesAsync().ConfigureAwait(false) > 0;
    }

    public async Task<(int, DateTime)?> GetLastRating(int userId, int ratingId, CancellationToken cancellationToken = default)
    {
        var query = _db.Ratings
            .Where(x => x.UserId == userId && x.RatingId == ratingId)
            .OrderByDescending(x => x.DateTime);
        var ratingData = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (ratingData == null)
            return null;

        return (ratingData.Rating, ratingData.DateTime);
    }
}
