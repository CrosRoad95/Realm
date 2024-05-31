namespace RealmCore.Tests.Unit.Players;

public class PlayersTests
{
    [Fact]
    public async Task DestroyingElementShouldResetCurrentInteractElement()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();

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
    public async Task YouShouldBeAbleAttachObjectToPlayer()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();
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
    public async Task AttachedElementShouldBeRemovedIfElementGetsDestroyed()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();
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
    public async Task ElementOwnerShouldWorkTests()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();

        worldObject.TrySetOwner(player);
        await hosting.DisconnectPlayer(player);

        worldObject.Owner.Should().BeNull();
    }

    [Fact]
    public async Task TestAsyncBindsCooldown()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
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
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        player.SetBindAsync("x", (player, keyState, token) =>
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
    public async Task PlayerShouldBeAbleToFightWhenAtLeastOneFlagIsEnabled()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        #endregion

        #region Act
        player.AddEnableFightFlag(1);
        #endregion

        #region Assert
        player.Controls.FireEnabled.Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task PlayerShouldNotBeAbleToFightWhenNoFlagIsSet()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
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
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
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
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
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
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var _ = Task.Run(async () =>
        {
            await Task.Delay(200);

            await hosting.DisconnectPlayer(player);
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
    public async Task SettingsShouldWork()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        #endregion

        #region Act
        var settings = player.Settings;
        settings.Set(1, "foo");
        bool hasSetting = settings.TryGet(1, out var settingValue);
        settings.TryGet(1, out var gotSettingValue);
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
    public async Task UpgradesShouldWork()
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
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


    [InlineData("TestPlayer1", true)]
    [InlineData("Testplayer", false)]
    [InlineData("FooPlayer", false)]
    [Theory]
    public async Task TryGetPlayerByNameTests(string nick, bool shouldExists)
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var elementSearchService = player.GetRequiredService<IElementSearchService>();
        #endregion

        #region Act
        bool found = elementSearchService.TryGetPlayerByName(nick, out var foundPlayer, PlayerSearchOption.None);
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
    public async Task SearchPlayersByNameTests(string pattern, bool shouldExists)
    {
        #region Arrange
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var searchService = player.GetRequiredService<IElementSearchService>();
        #endregion

        #region Act
        var found = searchService.SearchPlayers(pattern, new(PlayerSearchOption.None));
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
    public async Task UserVersionShouldWorkAsExpected()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var user = player.User;

        user.GetVersion().Should().Be(0);

        user.IncreaseVersion();
        user.GetVersion().Should().Be(1);
        user.TryFlushVersion(1).Should().BeTrue();
        user.GetVersion().Should().Be(0);
        user.TryFlushVersion(1).Should().BeFalse();
    }
}
