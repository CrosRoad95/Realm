using SlipeServer.Server.Elements.Enums;

namespace RealmCore.Tests.Tests.Components;

[Collection("Sequential")]
public class PlayerElementComponentTests
{
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;

    public PlayerElementComponentTests()
    {
        _server = new();
        _entityHelper = new(_server);
    }

    [Fact]
    public async Task TestAsyncBindsCooldown()
    {
        #region Arrange
        int executionCount = 0;
        var playerEntity = _entityHelper.CreatePlayerEntity();
        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBind("x", (entity, keyState) =>
        {
            executionCount++;
        });
        #endregion

        #region Act
        await playerElementComponent.InternalHandleBindExecuted("x", KeyState.Down);
        await playerElementComponent.InternalHandleBindExecuted("x", KeyState.Down);
        #endregion

        #region Assert
        executionCount.Should().Be(1);
        playerElementComponent.IsCooldownActive("x").Should().BeTrue();
        #endregion
    }

    [Fact]
    public async Task TestAsyncBindsThrowingException()
    {
        #region Arrange
        var playerEntity = _entityHelper.CreatePlayerEntity();
        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBindAsync("x", (entity, keyState) =>
        {
            throw new Exception("test123");
        });
        #endregion

        #region Act
        var action = async () => await playerElementComponent.InternalHandleBindExecuted("x", KeyState.Down);
        #endregion

        #region Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("test123");
        playerElementComponent.IsCooldownActive("x").Should().BeTrue();
        #endregion
    }

    [Fact]
    public void PlayerShouldBeAbleToFightWhenAtLeastOneFlagIsEnabled()
    {
        #region Arrange

        var playerEntity = _entityHelper.CreatePlayerEntity();
        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        #endregion

        #region Act
        playerElementComponent.AddEnableFightFlag(1);
        #endregion

        #region Assert
        playerElementComponent.Player.Controls.FireEnabled.Should().BeTrue();
        #endregion
    }

    [Fact]
    public void PlayerShouldNotBeAbleToFightWhenNoFlagIsSet()
    {
        #region Arrange

        var playerEntity = _entityHelper.CreatePlayerEntity();
        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        #endregion

        #region Act
        playerElementComponent.AddEnableFightFlag(1);
        playerElementComponent.RemoveEnableFightFlag(1);
        #endregion

        #region Assert
        playerElementComponent.Player.Controls.FireEnabled.Should().BeFalse();
        #endregion
    }
}
