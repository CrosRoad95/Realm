namespace Realm.Persistance.Interfaces;

public interface IJobRepository : IDisposable
{
    Task Commit();
    IQueryable<JobStatistics> GetAll();
}
