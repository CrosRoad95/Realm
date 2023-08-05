namespace RealmCore.Persistence.Repository;

internal class OpinionRepository : IOpinionRepository
{
    private readonly IDb _db;

    public OpinionRepository(IDb db)
    {
        _db = db;
    }

    public async Task AddOpinion(int userId, int opinionId, string opinion, DateTime dateTime)
    {
        _db.Opinions.Add(new OpinionData
        {
            UserId = userId,
            OpinionId = opinionId,
            Opinion = opinion,
            DateTime = dateTime
        });

        await _db.SaveChangesAsync();
    }

    public async Task<DateTime?> GetLastOpinionDateTime(int userId, int opinionId)
    {
        var query = _db.Opinions
            .Where(x => x.UserId == userId && x.OpinionId == opinionId)
            .OrderByDescending(x => x.DateTime);
        var opinionData = await query.FirstOrDefaultAsync();

        if (opinionData == null)
            return null;

        return opinionData.DateTime;
    }
}
