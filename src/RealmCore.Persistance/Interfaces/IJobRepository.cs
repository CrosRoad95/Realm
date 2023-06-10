namespace RealmCore.Persistence.Interfaces;

public interface IJobRepository : IRepositoryBase
{
    Task<Dictionary<int, UserJobStatisticsDTO>> GetJobStatistics(short jobId, int limit = 10);
    Task<JobStatisticsDTO?> GetUserJobStatistics(int userId, short jobId);
}
