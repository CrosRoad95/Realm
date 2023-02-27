namespace Realm.Server.Interfaces;

public interface IJobService
{
    public struct UserJobStatistics
    {
        public int points;
        public int timePlayed;
    }

    public struct JobStatistics
    {
        public int userId;
        public int points;
        public int timePlayed;
    }

    Task<UserJobStatistics> GetTotalUserJobStatistics(int userId, int jobId);
    Task<List<JobStatistics>> GetTotalJobStatistics(int jobId);
}
