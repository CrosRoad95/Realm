namespace RealmCore.Tests.Tests.Components;

public class FocusableComponentTests
{
    [Fact]
    public void FocusableComponentShouldWork()
    {
        Entity entity1 = new();
        Entity entity2 = new();
        var focusableComponent = entity1.AddComponent<FocusableComponent>();
        focusableComponent.AddFocusedPlayer(entity2);

        focusableComponent.FocusedPlayerCount.Should().Be(1);
        focusableComponent.FocusedPlayers.Should().BeEquivalentTo(entity2);
    }

    [Fact]
    public void YouShouldNotBeAbleToFocusItself()
    {
        Entity entity = new();
        var focusableComponent = entity.AddComponent<FocusableComponent>();
        var act = () => focusableComponent.AddFocusedPlayer(entity);

        act.Should().Throw<InvalidOperationException>();
        focusableComponent.FocusedPlayerCount.Should().Be(0);
    }

    [Fact]
    public void FocusedEntityShouldBeRemovedWhenItDisposes()
    {
        Entity entity1 = new();
        Entity entity2 = new();
        var focusableComponent = entity1.AddComponent<FocusableComponent>();

        focusableComponent.AddFocusedPlayer(entity2);
        entity2.Dispose();

        focusableComponent.FocusedPlayerCount.Should().Be(0);
    }
    
    [Fact]
    public void YouCanNotFocusOneEntityTwoTimes()
    {
        Entity entity1 = new();
        Entity entity2 = new();
        var focusableComponent = entity1.AddComponent<FocusableComponent>();
        var act = () => focusableComponent.AddFocusedPlayer(entity2);

        act().Should().BeTrue();
        act().Should().BeFalse();
        focusableComponent.FocusedPlayerCount.Should().Be(1);
    }
    
    [Fact]
    public void YouShouldBeAbleToRemoveFocusedPlayer()
    {
        Entity entity1 = new();
        Entity entity2 = new();
        var focusableComponent = entity1.AddComponent<FocusableComponent>();

        focusableComponent.AddFocusedPlayer(entity2).Should().BeTrue();
        focusableComponent.RemoveFocusedPlayer(entity2).Should().BeTrue();
        focusableComponent.RemoveFocusedPlayer(entity2).Should().BeFalse();
        focusableComponent.FocusedPlayerCount.Should().Be(0);
    }
    
    
    [Fact]
    public void RemovedFocusedComponentShouldWork()
    {
        Entity entity1 = new();
        Entity entity2 = new();
        var focusableComponent = entity1.AddComponent<FocusableComponent>();

        bool lostFocus = false;
        focusableComponent.PlayerLostFocus += (s, e) =>
        {
            if (e == entity2)
                lostFocus = true;
        };

        focusableComponent.AddFocusedPlayer(entity2);
        entity1.DestroyComponent(focusableComponent);
        lostFocus.Should().BeTrue();
    }

}
