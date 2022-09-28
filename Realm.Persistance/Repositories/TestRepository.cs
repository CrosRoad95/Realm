namespace Realm.Persistance.Repositories;

internal class TestRepository : ITestRepository
{
    private readonly IDb _db;

    public TestRepository(IDb db)
    {
        _db = db;
    }

    public async Task AddTest(Test test)
    {
        _db.Tests.Add(test);
        await _db.SaveChangesAsync();
    }
}
