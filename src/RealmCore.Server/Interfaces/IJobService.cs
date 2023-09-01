using RealmCore.Server.Structs;

namespace RealmCore.Server.Interfaces;

public interface IJobService
{
    Task<Dictionary<int, JobStatistics>> GetTotalJobStatistics(short jobId, int limit = 10);
    Task<JobStatistics?> TryGetTotalUserJobStatistics(int userId, short jobId);
}
