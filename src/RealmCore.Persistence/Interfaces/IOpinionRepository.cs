namespace RealmCore.Persistence.Interfaces;

public interface IOpinionRepository
{
    Task<bool> AddOpinion(int userId, int opinionId, string opinion, DateTime dateTime, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastOpinionDateTime(int userId, int opinionId, CancellationToken cancellationToken = default);
}
