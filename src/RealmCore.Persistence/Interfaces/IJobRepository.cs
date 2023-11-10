namespace RealmCore.Persistence.Interfaces;

public interface IJobRepository
{
    Task<List<UserJobStatisticsDTO>> GetJobStatistics(short jobId, int limit = 10, CancellationToken cancellationToken = default);
    Task<UserJobStatisticsDTO?> GetUserJobStatistics(int userId, short jobId, CancellationToken cancellationToken = default);
}
