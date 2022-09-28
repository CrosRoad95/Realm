namespace Realm.Server;

internal class Startup
{
    private readonly ITestRepository _testRepository;

    public Startup(ITestRepository testRepository)
    {
        _testRepository = testRepository;
    }

    public async Task StartAsync()
    {
        await _testRepository.AddTest(new Persistance.Entities.Test
        {
            Number = 123,
            Text = "Realm",
        });

    }
}
