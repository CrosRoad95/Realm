using SlipeServer.Server.Elements.Enums;

namespace RealmCore.Tests.Tests.Components;

[Collection("Sequential")]
public class PlayerElementComponentTests
{
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
            player.Destroy();
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
