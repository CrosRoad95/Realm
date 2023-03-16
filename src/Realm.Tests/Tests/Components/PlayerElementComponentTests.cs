using Realm.Domain.Components.Elements;
using SlipeServer.Server.Elements.Enums;

namespace Realm.Tests.Tests.Components;

public class PlayerElementComponentTests
{
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;

    public PlayerElementComponentTests()
    {
        _server = new();
        _entityHelper = new(_server);
    }

    //[Fact]
    public async Task TestBindsCooldown()
    {
        #region Arrange
        int executionCount = 0;
        var playerEntity = _entityHelper.CreatePlayerEntity();
        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBind("x", (entity, keyState) =>
        {
            executionCount++;
            return Task.CompletedTask;
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

    //[Fact]
    public async Task TestBindsThrowingException()
    {
        #region Arrange
        var playerEntity = _entityHelper.CreatePlayerEntity();
        var playerElementComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        playerElementComponent.SetBind("x", (entity, keyState) =>
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
}
