namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class AFKComponent : Component
{
    [Inject]
    private AFKService AFKService { get; set; } = default!;

    public DateTime? LastAFK { get; private set; }
    public bool IsAFK { get; private set; }
    public event Action<Entity, bool, TimeSpan>? StateChanged;

    public AFKComponent()
    {
    }

    protected override void Load()
    {
        AFKService.PlayerAFKStarted += HandlePlayerAFKStarted;
        AFKService.PlayerAFKSStopped += HandlePlayerAFKSStopped;
    }

    public override void Dispose()
    {
        base.Dispose();
        AFKService.PlayerAFKStarted -= HandlePlayerAFKStarted;
        AFKService.PlayerAFKSStopped -= HandlePlayerAFKSStopped;
    }

    protected virtual void StateHasChanged()
    {
        TimeSpan elapsed = (TimeSpan)((LastAFK == null) ? TimeSpan.Zero : DateTime.Now - LastAFK);
        StateChanged?.Invoke(Entity, IsAFK, elapsed);
    }

    private void HandlePlayerAFKSStopped(Player player)
    {
        if(!Entity.GetRequiredComponent<PlayerElementComponent>().Compare(player))
            return;

        IsAFK = false;
        StateHasChanged();
        LastAFK = DateTime.Now;
    }

    private void HandlePlayerAFKStarted(Player player)
    {
        if (!Entity.GetRequiredComponent<PlayerElementComponent>().Compare(player))
            return;

        IsAFK = true;
        StateHasChanged();
        LastAFK = DateTime.Now;
    }
}
