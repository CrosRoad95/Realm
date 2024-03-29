﻿using RealmCore.Server.Modules.Search;
using SlipeServer.Packets.Lua.Camera;

namespace RealmCore.Tests.Unit.Players;

public class PlayersTests : RealmUnitTestingBase
{
    [Fact]
    public void DestroyingElementShouldResetCurrentInteractElement()
    {
        #region Arrange
        var server = CreateServer();
        var player = CreatePlayer();
        var worldObject = server.CreateObject();

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
    public void YouShouldBeAbleAttachObjectToPlayer()
    {
        #region Arrange
        var server = CreateServer();
        var player = CreatePlayer();
        var worldObject = server.CreateObject();
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
    public void AttachedElementShouldBeRemovedIfElementGetsDestroyed()
    {
        #region Arrange
        var server = CreateServer();
        var player = CreatePlayer();
        var worldObject = server.CreateObject();
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
    public void ElementOwnerShouldWorkTests()
    {
        var server = CreateServer();
        var player = CreatePlayer();
        var worldObject = server.CreateObject();

        worldObject.TrySetOwner(player);
        player.Destroy();

        worldObject.Owner.Should().BeNull();
    }

    [Fact]
    public async Task TestAsyncBindsCooldown()
    {
        #region Arrange
        var player = CreateServerWithOnePlayer();
        int executionCount = 0;
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
        var player = CreateServerWithOnePlayer();
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
        var player = CreateServerWithOnePlayer();
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
        var player = CreateServerWithOnePlayer();
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
        var player = CreateServerWithOnePlayer();
        #endregion

        #region Act & Assert
        await player.FadeCameraAsync(CameraFade.Out, 0.1f);
        await player.FadeCameraAsync(CameraFade.In, 0.1f);
        #endregion
    }

    [Fact]
    public async Task FadeCameraShouldBeCancelable()
    {
        #region Arrange
        var player = CreateServerWithOnePlayer();
        var cancellationTokenSource = new CancellationTokenSource(100);
        #endregion

        #region Act
        var act = async () => await player.FadeCameraAsync(CameraFade.Out, 10.0f, cancellationTokenSource.Token);
        #endregion

        #region Asset
        await act.Should().ThrowAsync<OperationCanceledException>();
        #endregion
    }

    [Fact]
    public async Task FadeCameraShouldBeCanceledWhenPlayerQuit()
    {
        #region Arrange
        var player = CreateServerWithOnePlayer();

        var _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            player.TriggerDisconnected(QuitReason.Quit);
        });
        #endregion

        #region Act
        var act = async () => await player.FadeCameraAsync(CameraFade.Out, 2.0f);
        #endregion

        #region Asset
        await act.Should().ThrowAsync<OperationCanceledException>();
        #endregion
    }

    [Fact]
    public void SettingsShouldWork()
    {
        #region Arrange
        var player = CreateServerWithOnePlayer();
        #endregion

        #region Act
        var settings = player.Settings;
        settings.Set(1, "foo");
        bool hasSetting = settings.TryGet(1, out var settingValue);
        string gotSettingValue = settings.Get(1);
        bool removedSetting = settings.TryRemove(1);
        bool exists = settings.Has(1);
        #endregion

        #region Assert
        hasSetting.Should().BeTrue();
        settingValue.Should().Be("foo");
        gotSettingValue.Should().Be("foo");
        removedSetting.Should().BeTrue();
        exists.Should().BeFalse();
        #endregion
    }

    [Fact]
    public void UpgradesShouldWork()
    {
        #region Arrange
        var player = CreateServerWithOnePlayer();
        #endregion

        #region Act
        var upgrades = player.Upgrades;
        var hasSomeUpgrade1 = upgrades.Has(1);
        var added1 = upgrades.TryAdd(1);
        var added2 = upgrades.TryAdd(1);
        var hasSomeUpgrade2 = upgrades.Has(1);
        var removed1 = upgrades.TryRemove(1);
        var removed2 = upgrades.TryRemove(1);
        var hasSomeUpgrade3 = upgrades.Has(1);
        #endregion

        #region Assert
        hasSomeUpgrade1.Should().BeFalse();
        added1.Should().BeTrue();
        added2.Should().BeFalse();
        hasSomeUpgrade2.Should().BeTrue();
        removed1.Should().BeTrue();
        removed2.Should().BeFalse();
        hasSomeUpgrade3.Should().BeFalse();
        #endregion
    }


    [InlineData("TestPlayer", true)]
    [InlineData("Testplayer", false)]
    [InlineData("FooPlayer", false)]
    [Theory]
    public void TryGetPlayerByNameTests(string nick, bool shouldExists)
    {
        #region Arrange
        var player = CreateServerWithOnePlayer();

        var playersService = player.GetRequiredService<IPlayersService>();
        #endregion

        #region Act
        bool found = playersService.TryGetPlayerByName(nick, out var foundPlayer, PlayerSearchOption.None);
        #endregion

        #region Assert
        if (shouldExists)
        {
            found.Should().BeTrue();
            (player == foundPlayer).Should().BeTrue();
        }
        else
        {
            found.Should().BeFalse();
            foundPlayer.Should().BeNull();
        }
        #endregion
    }

    [InlineData("test", true)]
    [InlineData("asd", false)]
    [Theory]
    public void SearchPlayersByNameTests(string pattern, bool shouldExists)
    {
        #region Arrange
        var player = CreateServerWithOnePlayer();

        var searchService = player.GetRequiredService<IElementSearchService>();
        #endregion

        #region Act
        var found = searchService.SearchPlayers(pattern, PlayerSearchOption.None);
        #endregion

        #region Assert
        if (shouldExists)
        {
            var foundPlayer = found.First();
            (player == foundPlayer).Should().BeTrue();
        }
        else
        {
            found.Should().BeEmpty();
        }
        #endregion
    }

    [Fact]
    public void OnlyOnePlayerOfGivenNameShouldBeAbleToJoinServer()
    {
        CreateServer();

        var player1 = CreatePlayer("TestPlayer123");
        var player2 = CreatePlayer("TestPlayer123");

        player1.IsDestroyed.Should().BeFalse();
        player2.IsDestroyed.Should().BeTrue();
    }
}
