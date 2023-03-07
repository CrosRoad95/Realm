namespace Realm.Server.Interfaces;

public interface IJobService
{
    Task<Dictionary<int, JobStatistics>> GetTotalJobStatistics(short jobId, int limit = 10);
    Task<JobStatistics?> TryGetTotalUserJobStatistics(int userId, short jobId);

    public struct JobStatistics
    {
        public int points;
        public int timePlayed;
    }

}
