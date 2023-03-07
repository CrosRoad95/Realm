namespace Realm.Persistance.Interfaces;

public interface IJobRepository : IRepositoryBase
{
    Task<Dictionary<int, PlayerJobStatisticsDTO>> GetJobStatistics(short jobId, int limit = 10);
    Task<JobStatisticsDTO?> GetPlayerJobStatistics(int userId, short jobId);
}
