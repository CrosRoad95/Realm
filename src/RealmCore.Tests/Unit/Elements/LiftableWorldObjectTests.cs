namespace RealmCore.Tests.Unit.Elements;

public class LiftableWorldObjectTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public LiftableWorldObjectTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
    }

    [Fact]
    public void YouShouldBeAbleToLiftElementAndDropElement()
    {
        var worldObject = _hosting.CreateObject();

        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        using var monitored = interaction.Monitor();
        var result1 = interaction.TryLift(_player);
        var wasOwner = interaction.Owner;
        var result2 = interaction.TryDrop();

        using var _ = new AssertionScope();
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        wasOwner.Should().Be(_player);
        monitored.Should().Raise(nameof(LiftableInteraction.Lifted));
        monitored.Should().Raise(nameof(LiftableInteraction.Dropped));
    }

    [Fact]
    public void ElementCanBeLiftedOnce()
    {
        var worldObject = _hosting.CreateObject();

        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        interaction.TryLift(_player);
        var result = interaction.TryLift(_player);

        result.Should().BeFalse();
    }

    [Fact]
    public void ElementCanBeDroppedOnce()
    {
        var worldObject = _hosting.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        interaction.TryLift(_player);
        interaction.TryDrop();
        var result = interaction.TryDrop();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ElementShouldBeDroppedUponDispose()
    {
        var worldObject = _hosting.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        using var monitored = interaction.Monitor();
        interaction.TryLift(_player);

        await _hosting.DisconnectPlayer(_player);

        using var _ = new AssertionScope();
        interaction.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(interaction.Lifted));
        monitored.Should().Raise(nameof(interaction.Dropped));
    }

    [Fact]
    public async Task ElementShouldBeDroppedUponDispose2()
    {
        var worldObject = _hosting.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        using var monitored = interaction.Monitor();
        interaction.TryLift(_player);

        await _hosting.DisconnectPlayer(_player);

        using var _ = new AssertionScope();
        interaction.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(interaction.Lifted));
        monitored.Should().Raise(nameof(interaction.Dropped));
    }

    [Fact]
    public async Task OnlyWhitelistedEntitiesShouldBeAbleToLiftOtherElement()
    {
        var player2 = await _hosting.CreatePlayer();
        var worldObject = _hosting.CreateObject();

        var interaction1 = new LiftableInteraction();
        var interaction2 = new LiftableInteraction(_player);

        bool lifted1 = interaction1.TryLift(_player);
        bool dropped1 = interaction1.TryDrop();
        bool lifted2 = interaction2.TryLift(player2);

        using var _ = new AssertionScope();
        lifted1.Should().BeTrue();
        dropped1.Should().BeTrue();
        lifted2.Should().BeFalse();
    }
}
