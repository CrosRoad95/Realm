using RealmCore.Server.Components.Object;
using RealmCore.Tests.Classes.Components;

namespace RealmCore.Tests.Tests.Components;

public class LiftableWorldObjectComponentTests
{
    [Fact]
    public void YouShouldBeAbleToLiftElementAndDropElement()
    {
        #region Act
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        var LiftableWorldObjectComponent = worldObject.AddComponent<LiftableWorldObjectComponent>();

        using var monitored = LiftableWorldObjectComponent.Monitor();
        var result1 = LiftableWorldObjectComponent.TryLift(player);
        var wasOwner = LiftableWorldObjectComponent.Owner;
        var result2 = LiftableWorldObjectComponent.TryDrop();
        #endregion

        #region Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        wasOwner.Should().Be(player);
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Lifted));
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Dropped));
        #endregion
    }

    [Fact]
    public void ElementCanBeLiftedOnce()
    {
        #region Act
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        var LiftableWorldObjectComponent = worldObject.AddComponent<LiftableWorldObjectComponent>();

        LiftableWorldObjectComponent.TryLift(player);
        var result = LiftableWorldObjectComponent.TryLift(player);
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
        var LiftableWorldObjectComponent = worldObject.AddComponent<LiftableWorldObjectComponent>();

        #region Act
        LiftableWorldObjectComponent.TryLift(player);
        LiftableWorldObjectComponent.TryDrop();
        var result = LiftableWorldObjectComponent.TryDrop();
        #endregion

        #region Assert
        result.Should().BeFalse();
        #endregion
    }

    [Fact]
    public void YouCanNotLiftYourself()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var LiftableWorldObjectComponent = player.AddComponent<LiftableWorldObjectComponent>();

        #region Act
        var result = LiftableWorldObjectComponent.TryLift(LiftableWorldObjectComponent.Element);
        #endregion

        #region Assert
        result.Should().BeFalse();
        #endregion
    }

    [Fact]
    public void ElementhouldBeDroppedUponDispose()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        var LiftableWorldObjectComponent = worldObject.AddComponent<LiftableWorldObjectComponent>();
        #region Arrange
        using var monitored = LiftableWorldObjectComponent.Monitor();
        LiftableWorldObjectComponent.TryLift(player);
        #endregion

        #region Act
        player.Destroy();
        #endregion

        #region Assert
        LiftableWorldObjectComponent.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Lifted));
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Dropped));
        #endregion
    }

    [Fact]
    public void ElementShouldBeDroppedUponDispose2()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        var LiftableWorldObjectComponent = worldObject.AddComponent<LiftableWorldObjectComponent>();

        #region Arrange
        using var monitored = LiftableWorldObjectComponent.Monitor();
        LiftableWorldObjectComponent.TryLift(player);
        #endregion

        #region Act
        player.Destroy();
        #endregion

        #region Assert
        LiftableWorldObjectComponent.Owner.Should().BeNull();
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Lifted));
        monitored.Should().Raise(nameof(LiftableWorldObjectComponent.Dropped));
        #endregion
    }

    [Fact]
    public void OnlyWhitelistedEntitiesShouldBeAbleToLiftOtherElement()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player1 = realmTestingServer.CreatePlayer();
        var player2 = realmTestingServer.CreatePlayer();
        var player3 = realmTestingServer.CreatePlayer();

        player1.AddComponent<TestElementComponent>();
        var liftableWorldObjectComponent = player1.AddComponent(new LiftableWorldObjectComponent(player2));
        #endregion

        #region Act
        bool lifted1 = liftableWorldObjectComponent.TryLift(player2);
        if (lifted1)
            liftableWorldObjectComponent.TryDrop();
        bool lifted2 = liftableWorldObjectComponent.TryLift(player3);
        #endregion

        #region Assert
        lifted1.Should().BeTrue();
        lifted2.Should().BeFalse();
        #endregion
    }
}
