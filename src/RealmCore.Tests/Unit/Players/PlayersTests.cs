namespace RealmCore.Tests.Unit.Players;

public class PlayersTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingServerHosting<RealmTestingPlayer> _hosting;
    private readonly RealmTestingPlayer _player;

    public PlayersTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _hosting = fixture.Hosting;
    }

    [Fact]
    public void DestroyingElementShouldResetCurrentInteractElement()
    {
        var worldObject = _hosting.CreateObject();

        _player.CurrentInteractElement = worldObject;
        _player.CurrentInteractElement.Should().Be(worldObject);

        worldObject.Destroy();

        _player.CurrentInteractElement.Should().BeNull();
    }

    [Fact]
    public void YouShouldBeAbleAttachObjectToPlayer()
    {
        var worldObject = _hosting.CreateObject();

        var attached = _player.Attach(worldObject, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null);

        attached.Should().BeTrue();
        _player.AttachedBoneElementsCount.Should().Be(1);
    }

    [Fact]
    public void AttachedElementShouldBeRemovedIfElementGetsDestroyed()
    {
        var worldObject = _hosting.CreateObject();

        _player.Attach(worldObject, SlipeServer.Packets.Enums.BoneId.Pelvis, null, null);
        worldObject.Destroy();

        _player.AttachedBoneElements.Where(x => x.WorldObject == worldObject).Should().HaveCount(0);
    }

    [Fact]
    public async Task ElementOwnerShouldWorkTests()
    {
        var player = await _hosting.CreatePlayer();
        var worldObject = _hosting.CreateObject();

        worldObject.TrySetOwner(player);
        await _hosting.DisconnectPlayer(player);

        worldObject.Owner.Should().BeNull();
    }

    [Fact]
    public async Task TestAsyncBindsCooldown()
    {
        int executionCount = 0;
        _player.SetBind("y", (player, keyState) =>
        {
            executionCount++;
        });

        await _player.InternalHandleBindExecuted("y", KeyState.Down);
        await _player.InternalHandleBindExecuted("y", KeyState.Down);

        executionCount.Should().Be(1);
        _player.IsCooldownActive("y").Should().BeTrue();
    }

    [Fact]
    public async Task TestAsyncBindsThrowingException()
    {
        _player.SetBindAsync("x", (player, keyState, token) =>
        {
            throw new Exception("test123");
        });

        var action = async () => await _player.InternalHandleBindExecuted("x", KeyState.Down);

        await action.Should().ThrowAsync<Exception>().WithMessage("test123");
        _player.IsCooldownActive("x").Should().BeTrue();
    }

    [Fact]
    public void PlayerShouldBeAbleToFightWhenAtLeastOneFlagIsEnabled()
    {
        _player.AddEnableFightFlag(1);

        _player.Controls.FireEnabled.Should().BeTrue();
    }

    [Fact]
    public void PlayerShouldNotBeAbleToFightWhenNoFlagIsSet()
    {
        _player.AddEnableFightFlag(1);
        _player.RemoveEnableFightFlag(1);

        _player.Controls.FireEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task FadeCameraShouldWork()
    {
        await _player.FadeCameraAsync(CameraFade.Out, 0.1f);
        await _player.FadeCameraAsync(CameraFade.In, 0.1f);
    }

    [Fact]
    public async Task FadeCameraShouldBeCancelable()
    {
        var cancellationTokenSource = new CancellationTokenSource(100);

        var act = async () => await _player.FadeCameraAsync(CameraFade.Out, 10.0f, cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task FadeCameraShouldBeCanceledWhenPlayerQuit()
    {
        var player = await _hosting.CreatePlayer();

        var _ = Task.Run(async () =>
        {
            await Task.Delay(200);

            await _hosting.DisconnectPlayer(player);
        });

        var act = async () => await player.FadeCameraAsync(CameraFade.Out, 2.0f);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void SettingsShouldWork()
    {
        var settings = _player.Settings;
        settings.Set(1, "foo");
        bool hasSetting = settings.TryGet(1, out var settingValue);
        settings.TryGet(1, out var gotSettingValue);
        bool removedSetting = settings.TryRemove(1);
        bool exists = settings.Has(1);

        hasSetting.Should().BeTrue();
        settingValue.Should().Be("foo");
        gotSettingValue.Should().Be("foo");
        removedSetting.Should().BeTrue();
        exists.Should().BeFalse();
    }

    [Fact]
    public void UpgradesShouldWork()
    {
        var upgrades = _player.Upgrades;
        var hasSomeUpgrade1 = upgrades.Has(1);
        var added1 = upgrades.TryAdd(1);
        var added2 = upgrades.TryAdd(1);
        var hasSomeUpgrade2 = upgrades.Has(1);
        var removed1 = upgrades.TryRemove(1);
        var removed2 = upgrades.TryRemove(1);
        var hasSomeUpgrade3 = upgrades.Has(1);

        hasSomeUpgrade1.Should().BeFalse();
        added1.Should().BeTrue();
        added2.Should().BeFalse();
        hasSomeUpgrade2.Should().BeTrue();
        removed1.Should().BeTrue();
        removed2.Should().BeFalse();
        hasSomeUpgrade3.Should().BeFalse();
    }


    [InlineData("TestPlayer1", true)]
    [InlineData("FooPlayer", false)]
    [Theory]
    public void TryGetPlayerByNameTests(string nick, bool shouldExists)
    {
        var elementSearchService = _player.GetRequiredService<PlayerSearchService>();

        bool found = elementSearchService.TryGetPlayerByName(nick, out var foundPlayer, PlayerSearchOption.None);

        if (shouldExists)
        {
            found.Should().BeTrue();
            (_player == foundPlayer).Should().BeTrue();
        }
        else
        {
            found.Should().BeFalse();
            foundPlayer.Should().BeNull();
        }
    }

    [InlineData("test", true)]
    [InlineData("asd", false)]
    [Theory]
    public void SearchPlayersByNameTests(string pattern, bool shouldExists)
    {
        var searchService = _player.GetRequiredService<PlayerSearchService>();

        var found = searchService.SearchPlayers(pattern, new(PlayerSearchOption.None));

        if (shouldExists)
        {
            var foundPlayer = found.First();
            (_player == foundPlayer).Should().BeTrue();
        }
        else
        {
            found.Should().BeEmpty();
        }
    }

    [Fact]
    public void UserVersionShouldWorkAsExpected()
    {
        var user = _player.User;

        user.GetVersion().Should().Be(0);

        user.IncreaseVersion();
        user.GetVersion().Should().Be(1);
        user.TryFlushVersion(1).Should().BeTrue();
        user.GetVersion().Should().Be(0);
        user.TryFlushVersion(1).Should().BeFalse();
    }

    public void Dispose()
    {
        _player.User.TryFlushVersion(0);
        _player.Upgrades.Clear();
    }
}
