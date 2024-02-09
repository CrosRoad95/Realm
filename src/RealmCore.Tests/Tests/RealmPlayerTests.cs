using RealmCore.Tests.Providers;
using SlipeServer.Server.Elements.Enums;
using SlipeServer.Server.Enums;

namespace RealmCore.Tests.Tests;

public class RealmPlayerTests
{
    [Fact]
    public async Task SavingAndLoadingPlayerShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var now = realmTestingServer.TestDateTimeProvider.Now;
        {
            var player = realmTestingServer.CreatePlayer();
            player.Name = "foo";
            await realmTestingServer.SignInPlayer(player);

            player.Position = new Vector3(1, 2, 3);
            player.Money.Amount = 12.345m;
            player.DailyVisits.Update(now.AddHours(24));
            player.DailyVisits.VisitsInRow.Should().Be(2);
            player.Settings.Set(1, "test");
            player.Bans.Add(1);
            player.Upgrades.TryAdd(1);

            await realmTestingServer.GetRequiredService<ISaveService>().Save(player);
            player.TriggerDisconnected(QuitReason.Quit);

            player.Money.Amount.Should().Be(0);
        }

        realmTestingServer.TestDateTimeProvider.AddOffset(TimeSpan.FromDays(1));

        {
            var player = realmTestingServer.CreatePlayer();
            player.Name = "foo";
            await realmTestingServer.SignInPlayer(player);
            player.TrySpawnAtLastPosition();

            player.Position.Should().Be(new Vector3(1, 2, 3));
            player.Money.Amount.Should().Be(12.345m);
            player.DailyVisits.VisitsInRow.Should().Be(2);
            player.DailyVisits.LastVisit.Should().Be(realmTestingServer.TestDateTimeProvider.Now.Date);
            player.Settings.Get(1).Should().Be("test");
            player.Bans.IsBanned(1).Should().BeTrue();
            player.Upgrades.Has(1).Should().BeTrue();
        }
    }

    //[Fact]
    public async Task Test1()
    {
        var realmTestingServer = new RealmTestingServer();

        {
            var player = realmTestingServer.CreatePlayer();
            await realmTestingServer.SignInPlayer(player);

            for(int i = 0; i < 25; i++)
            {
                player.Events.Add(i % 5, $"test{i}");
            }
            player.TriggerDisconnected(QuitReason.Quit);
        }

        {
            var player = realmTestingServer.CreatePlayer();
            await realmTestingServer.SignInPlayer(player);
            var events = player.Events;
            var initialCount1 = events.Count();
            var fetched1 = await events.FetchMore();
            var fetched2 = await events.FetchMore();
            var initialCount2 = events.Count();
            ;
        }
    }

    [Fact]
    public void DestroyingComponentShouldReset()
    {
        #region Arrange
        TestDateTimeProvider testDateTimeProvider = new();
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        player.CurrentInteractElement = worldObject;
        player.CurrentInteractElement.Should().Be(worldObject);
        #endregion

        #region Act
        worldObject.Destroy();
        #endregion

        #region Assert
        player.CurrentInteractElement.Should().BeNull();
        #endregion
    }

    [Fact]
    public void DestroyingElementShouldResetAndRemoveComponent()
    {
        #region Arrange
        TestDateTimeProvider testDateTimeProvider = new();
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        player.CurrentInteractElement = worldObject;
        #endregion

        #region Act
        worldObject.Destroy();
        #endregion

        #region Assert
        player.CurrentInteractElement.Should().BeNull();
        #endregion
    }

    [Fact]
    public void YouShouldBeAbleAttachObjectToPlayer()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        #endregion

        #region Act
        var attached = player.Attach(worldObject, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null);
        #endregion

        #region Assert
        attached.Should().BeTrue();
        player.AttachedBoneElementsCount.Should().Be(1);
        #endregion
    }

    [Fact]
    public void AttachedElementComponentShouldBeRemovedIfElementGetsDestroyed()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();
        #endregion

        #region Act
        player.Attach(worldObject, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null);
        worldObject.Destroy();
        #endregion

        #region Assert
        player.AttachedBoneElementsCount.Should().Be(0);
        #endregion
    }

    [Fact]
    public void OwnerComponentTestsShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var worldObject = realmTestingServer.CreateObject();

        worldObject.TrySetOwner(player);
        player.Destroy();

        worldObject.Owner.Should().BeNull();
    }

    [Fact]
    public async Task TestAsyncBindsCooldown()
    {
        #region Arrange
        int executionCount = 0;
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        player.SetBind("x", (player, keyState) =>
        {
            executionCount++;
        });
        #endregion

        #region Act
        await player.InternalHandleBindExecuted("x", KeyState.Down);
        await player.InternalHandleBindExecuted("x", KeyState.Down);
        #endregion

        #region Assert
        executionCount.Should().Be(1);
        player.IsCooldownActive("x").Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task TestAsyncBindsThrowingException()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        player.SetBindAsync("x", (player, keyState) =>
        {
            throw new Exception("test123");
        });
        #endregion

        #region Act
        var action = async () => await player.InternalHandleBindExecuted("x", KeyState.Down);
        #endregion

        #region Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("test123");
        player.IsCooldownActive("x").Should().BeTrue();
        #endregion
    }

    [Fact]
    public void PlayerShouldBeAbleToFightWhenAtLeastOneFlagIsEnabled()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        #endregion

        #region Act
        player.AddEnableFightFlag(1);
        #endregion

        #region Assert
        player.Controls.FireEnabled.Should().BeTrue();
        #endregion
    }

    [Fact]
    public void PlayerShouldNotBeAbleToFightWhenNoFlagIsSet()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        #endregion

        #region Act
        player.AddEnableFightFlag(1);
        player.RemoveEnableFightFlag(1);
        #endregion

        #region Assert
        player.Controls.FireEnabled.Should().BeFalse();
        #endregion
    }

    [Fact]
    public async Task FadeCameraShouldWork()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        #endregion

        #region Act & Assert
        await player.FadeCameraAsync(SlipeServer.Packets.Lua.Camera.CameraFade.Out, 0.1f);
        await player.FadeCameraAsync(SlipeServer.Packets.Lua.Camera.CameraFade.In, 0.1f);
        #endregion
    }

    [Fact]
    public async Task FadeCameraShouldBeCancelable()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var cancellationTokenSource = new CancellationTokenSource(100);
        #endregion

        #region Act
        var act = async () => await player.FadeCameraAsync(SlipeServer.Packets.Lua.Camera.CameraFade.Out, 10.0f, cancellationTokenSource.Token);
        #endregion

        #region Asset
        await act.Should().ThrowAsync<OperationCanceledException>();
        #endregion
    }

    [Fact]
    public async Task FadeCameraShouldBeCanceledWhenPlayerQuit()
    {
        #region Arrange
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();

        var _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            player.TriggerDisconnected(QuitReason.Quit);
        });
        #endregion

        #region Act
        var act = async () => await player.FadeCameraAsync(SlipeServer.Packets.Lua.Camera.CameraFade.Out, 2.0f);
        #endregion

        #region Asset
        await act.Should().ThrowAsync<OperationCanceledException>();
        #endregion
    }
}
