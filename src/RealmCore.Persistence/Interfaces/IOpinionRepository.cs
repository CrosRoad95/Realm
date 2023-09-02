namespace RealmCore.Persistence.Interfaces;

public interface IOpinionRepository
{
    Task<bool> AddOpinion(int userId, int opinionId, string opinion, DateTime dateTime);
    Task<DateTime?> GetLastOpinionDateTime(int userId, int opinionId);
}
