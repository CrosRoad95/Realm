namespace RealmCore.Tests.Unit.Elements;

public class LiftableWorldObjectTests
{
    [Fact]
    public async Task YouShouldBeAbleToLiftElementAndDropElement()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();

        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        using var monitored = interaction.Monitor();
        var result1 = interaction.TryLift(player);
        var wasOwner = interaction.Owner;
        var result2 = interaction.TryDrop();

        result1.Should().BeTrue();
        result2.Should().BeTrue();
        wasOwner.Should().Be(player);
        monitored.Should().Raise(nameof(LiftableInteraction.Lifted));
        monitored.Should().Raise(nameof(LiftableInteraction.Dropped));
    }

    [Fact]
    public async Task ElementCanBeLiftedOnce()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();

        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        interaction.TryLift(player);
        var result = interaction.TryLift(player);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ElementCanBeDroppedOnce()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        #region Act
        interaction.TryLift(player);
        interaction.TryDrop();
        var result = interaction.TryDrop();
        #endregion

        #region Assert
        result.Should().BeFalse();
        #endregion
    }

    [Fact]
    public async Task ElementShouldBeDroppedUponDispose()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        #region Arrange
        using var monitored = interaction.Monitor();
        interaction.TryLift(player);
        #endregion

        #region Act
        player.Destroy();
        #endregion

        #region Assert
        interaction.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(interaction.Lifted));
        monitored.Should().Raise(nameof(interaction.Dropped));
        #endregion
    }

    [Fact]
    public async Task ElementShouldBeDroppedUponDispose2()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        #region Arrange
        using var monitored = interaction.Monitor();
        interaction.TryLift(player);
        #endregion

        #region Act
        player.Destroy();
        #endregion

        #region Assert
        interaction.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(interaction.Lifted));
        monitored.Should().Raise(nameof(interaction.Dropped));
        #endregion
    }

    [Fact]
    public async Task OnlyWhitelistedEntitiesShouldBeAbleToLiftOtherElement()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player1 = await hosting.CreatePlayer();
        var player2 = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();

        var interaction1 = new LiftableInteraction();
        var interaction2 = new LiftableInteraction(player1);
        #endregion

        #region Act
        bool lifted1 = interaction1.TryLift(player1);
        bool dropped1 = interaction1.TryDrop();
        bool lifted2 = interaction2.TryLift(player2);
        #endregion

        #region Assert
        lifted1.Should().BeTrue();
        dropped1.Should().BeTrue();
        lifted2.Should().BeFalse();
        #endregion
    }
}
