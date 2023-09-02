namespace RealmCore.Persistence.Repository;

internal sealed class OpinionRepository : IOpinionRepository
{
    private readonly IDb _db;

    public OpinionRepository(IDb db)
    {
        _db = db;
    }

    public async Task<bool> AddOpinion(int userId, int opinionId, string opinion, DateTime dateTime)
    {
        _db.Opinions.Add(new OpinionData
        {
            UserId = userId,
            OpinionId = opinionId,
            Opinion = opinion,
            DateTime = dateTime
        });

        return await _db.SaveChangesAsync().ConfigureAwait(false) == 1;
    }

    public async Task<DateTime?> GetLastOpinionDateTime(int userId, int opinionId)
    {
        var query = _db.Opinions
            .Where(x => x.UserId == userId && x.OpinionId == opinionId)
            .OrderByDescending(x => x.DateTime);

        var opinionData = await query.FirstOrDefaultAsync().ConfigureAwait(false);

        return opinionData?.DateTime;
    }
}
