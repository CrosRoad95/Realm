namespace RealmCore.Server.Logic.Components;

internal sealed class UserComponentLogic : ComponentLogic<UserComponent>
{
    private readonly IActiveUsers _activeUsers;

    public UserComponentLogic(IEntityEngine entityEngine, IActiveUsers activeUsers) : base(entityEngine)
    {
        _activeUsers = activeUsers;
    }

    protected override void ComponentDetached(UserComponent userComponent)
    {
        _activeUsers.TrySetInactive(userComponent.Id);
    }
}
