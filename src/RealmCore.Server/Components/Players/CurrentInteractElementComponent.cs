namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class CurrentInteractElementComponent : ComponentLifecycle
{
    public Element? CurrentInteractElement { get; private set; }
    private object _lock = new();
    public event Action<CurrentInteractElementComponent>? Disposed; 

    public CurrentInteractElementComponent(Element element)
    {
        CurrentInteractElement = element;

        CurrentInteractElement.Destroyed += HandleDestroyed;
    }

    private void HandleDestroyed(Element element)
    {
        lock (_lock)
        {
            if (CurrentInteractElement != null)
                CurrentInteractElement.Destroyed -= HandleDestroyed;
            CurrentInteractElement = null!;
        }


        ((IComponents)Element).Components.DestroyComponent(this);
    }

    public override void Dispose()
    {
        lock (_lock)
        {
            if (CurrentInteractElement != null)
                CurrentInteractElement.Destroyed -= HandleDestroyed;
            CurrentInteractElement = null!;
        }
        Disposed?.Invoke(this);
        base.Dispose();
    }
}
