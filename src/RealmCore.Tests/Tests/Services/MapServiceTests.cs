using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;

namespace RealmCore.Tests.Tests.Services;

public class MapServiceTests
{
    private readonly Mock<ILogger<MapsService>> _logger = new(MockBehavior.Strict);
    public MapServiceTests()
    {
        _logger.SetupLogger();
    }

    [Fact]
    public void YouCanNotRegisterEmptyMapFromMemory()
    {
        var mapsService = new MapsService(_logger.Object, null);

        var act = () => mapsService.RegisterMapFromMemory("empty", Enumerable.Empty<WorldObject>());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void YouCanNotAddTwoMapsWithTheSameName()
    {
        var mapsService = new MapsService(_logger.Object, null);

        var act = () => mapsService.RegisterMapFromMemory("duplicate", new WorldObject[] { new WorldObject(ObjectModel.Vegtree3, Vector3.Zero) });

        act.Should().NotThrow();
        act.Should().Throw<Exception>().WithMessage("Map of name 'duplicate' already exists");
    }
}
