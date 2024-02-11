namespace RealmCore.Server.Modules.Players.Gui.Dx;

public interface IPlayerHudFeature : IPlayerFeature
{
    IReadOnlyList<IHudLayer> Layers { get; }

    event Action<IPlayerHudFeature, IHudLayer>? LayerCreated;
    event Action<IPlayerHudFeature, IHudLayer>? LayerRemoved;

    bool AddLayer(IHudLayer hudLayer);
    THudLayer? AddLayer<THudLayer>(params object[] parameters) where THudLayer : IHudLayer;
    bool RemoveLayer(IHudLayer hudLayer);
    bool RemoveLayer<THudLayer>() where THudLayer : IHudLayer;
}

internal sealed class PlayerHudFeature : IPlayerHudFeature, IDisposable
{
    private readonly object _lock = new();

    private readonly List<IHudLayer> _hudLayers = [];

    public event Action<IPlayerHudFeature, IHudLayer>? LayerCreated;
    public event Action<IPlayerHudFeature, IHudLayer>? LayerRemoved;

    public IReadOnlyList<IHudLayer> Layers
    {
        get
        {
            lock (_lock)
            {
                return [.. _hudLayers];
            }
        }
    }

    public RealmPlayer Player { get; init; }

    public PlayerHudFeature(PlayerContext playerContext)
    {
        Player = playerContext.Player;
    }

    public bool AddLayer(IHudLayer hudLayer)
    {
        lock (_lock)
        {
            if (_hudLayers.Contains(hudLayer))
                return false;
            _hudLayers.Add(hudLayer);
        }

        LayerCreated?.Invoke(this, hudLayer);
        return true;
    }

    public THudLayer? AddLayer<THudLayer>(params object[] parameters) where THudLayer : IHudLayer
    {
        var layer = ActivatorUtilities.CreateInstance<THudLayer>(Player.ServiceProvider, parameters);
        if (AddLayer(layer))
            return layer;
        return default;
    }

    public bool RemoveLayer(IHudLayer hudLayer)
    {
        lock (_lock)
        {
            if (!_hudLayers.Remove(hudLayer))
                return false;
        }

        hudLayer.Dispose();
        LayerRemoved?.Invoke(this, hudLayer);
        return true;
    }

    public bool RemoveLayer<THudLayer>() where THudLayer : IHudLayer
    {
        IHudLayer? hudLayer;
        lock (_lock)
        {
            hudLayer = _hudLayers.OfType<THudLayer>().FirstOrDefault();
        }

        if (hudLayer == null)
            return false;
        return RemoveLayer(hudLayer);
    }

    public void Dispose()
    {
        foreach (var layer in Layers)
        {
            RemoveLayer(layer);
        }
    }
}
