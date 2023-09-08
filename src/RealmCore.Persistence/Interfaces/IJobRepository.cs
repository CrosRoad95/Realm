namespace RealmCore.Persistence.Interfaces;

public interface IJobRepository
{
    Task<Dictionary<int, UserJobStatisticsDTO>> GetJobStatistics(short jobId, int limit = 10, CancellationToken cancellationToken = default);
    Task<JobStatisticsDTO?> GetUserJobStatistics(int userId, short jobId, CancellationToken cancellationToken = default);
}
