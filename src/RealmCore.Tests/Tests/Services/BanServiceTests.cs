namespace RealmCore.Tests.Tests.Services;

public class BanServiceTests
{
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;
    public BanServiceTests()
    {
        var testConfigurationProvider = new TestConfigurationProvider(useSqlLite: true);
        _server = new(testConfigurationProvider);
        _entityHelper = new(_server);
    }

    [Fact]
    public async Task BanShouldWork()
    {
        await _server.GetRequiredService<IDb>().MigrateAsync();
        var banService = _server.GetRequiredService<IBanService>();
        var player = _entityHelper.CreatePlayerEntity();
        await _entityHelper.LogInEntity(player);


        await banService.Ban(player, reason: "sample reason");
        var isBanned = await banService.IsBanned(player);
        isBanned.Should().BeTrue();
    }

    [Fact]
    public async Task BanShouldFail()
    {
        await _server.GetRequiredService<IDb>().MigrateAsync();
        var banRepository = _server.GetRequiredService<IBanRepository>();
        var act = async () => await banRepository.CreateBanForUser(123);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
