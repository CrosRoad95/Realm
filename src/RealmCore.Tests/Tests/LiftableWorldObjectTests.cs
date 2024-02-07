using RealmCore.Server.Concepts.Interactions;

namespace RealmCore.Tests.Tests;

public class LiftableWorldObjectTests
{
    [Fact]
    public void YouShouldBeAbleToLiftElementAndDropElement()
    {
        #region Act
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        using var monitored = interaction.Monitor();
        var result1 = interaction.TryLift(player);
        var wasOwner = interaction.Owner;
        var result2 = interaction.TryDrop();
        #endregion

        #region Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        wasOwner.Should().Be(player);
        monitored.Should().Raise(nameof(LiftableInteraction.Lifted));
        monitored.Should().Raise(nameof(LiftableInteraction.Dropped));
        #endregion
    }

    [Fact]
    public void ElementCanBeLiftedOnce()
    {
        #region Act
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        var interaction = new LiftableInteraction();
        worldObject.Interaction = new LiftableInteraction();

        interaction.TryLift(player);
        var result = interaction.TryLift(player);
        #endregion

        #region Assert
        result.Should().BeFalse();
        #endregion
    }

    [Fact]
    public void ElementCanBeDroppedOnce()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
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
    public void ElementShouldBeDroppedUponDispose()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
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
    public void ElementShouldBeDroppedUponDispose2()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
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
    public void OnlyWhitelistedEntitiesShouldBeAbleToLiftOtherElement()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();

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
