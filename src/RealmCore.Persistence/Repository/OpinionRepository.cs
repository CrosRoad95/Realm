namespace RealmCore.Persistence.Repository;

public interface IOpinionRepository
{
    Task<bool> AddOpinion(int userId, int opinionId, string opinion, DateTime dateTime, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastOpinionDateTime(int userId, int opinionId, CancellationToken cancellationToken = default);
}

internal sealed class OpinionRepository : IOpinionRepository
{
    private readonly IDb _db;

    public OpinionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> AddOpinion(int userId, int opinionId, string opinion, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        _db.Opinions.Add(new OpinionData
        {
            UserId = userId,
            OpinionId = opinionId,
            Opinion = opinion,
            DateTime = dateTime
        });

        return await _db.SaveChangesAsync(cancellationToken) == 1;
    }

    public async Task<DateTime?> GetLastOpinionDateTime(int userId, int opinionId, CancellationToken cancellationToken = default)
    {
        var query = _db.Opinions
            .Where(x => x.UserId == userId && x.OpinionId == opinionId)
            .OrderByDescending(x => x.DateTime);

        var opinionData = await query.FirstOrDefaultAsync(cancellationToken);

        return opinionData?.DateTime;
    }
}
