namespace RealmCore.Server.Modules.Players.Gui.Dx;

public sealed class PlayerHudFeature : IPlayerFeature, IEnumerable<IHudLayer>, IDisposable
{
    private readonly object _lock = new();

    private readonly List<IHudLayer> _hudLayers = [];

    public event Action<PlayerHudFeature, IHudLayer>? LayerAdded;
    public event Action<PlayerHudFeature, IHudLayer>? LayerRemoved;

    public IHudLayer[] VisibleLayers
    {
        get
        {
            lock (_lock)
            {
                return _hudLayers.Where(x => x.Visible).ToArray();
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

        LayerAdded?.Invoke(this, hudLayer);
        return true;
    }

    public THudLayer? AddLayer<THudLayer>(params object[] parameters) where THudLayer : IHudLayer
    {
        var layer = ActivatorUtilities.CreateInstance<THudLayer>(Player.ServiceProvider, parameters);
        if (AddLayer(layer))
            return layer;
        return default;
    }

    public void RemoveLayer(IHudLayer hudLayer)
    {
        lock (_lock)
        {
            if (!_hudLayers.Remove(hudLayer))
                throw new HudLayerNotFoundException(hudLayer.GetType());
        }

        hudLayer.Dispose();
        LayerRemoved?.Invoke(this, hudLayer);
    }

    public void RemoveLayer<THudLayer>() where THudLayer : IHudLayer
    {
        THudLayer? hudLayer;
        lock (_lock)
        {
            hudLayer = _hudLayers.OfType<THudLayer>().FirstOrDefault();

            if (hudLayer == null || !_hudLayers.Remove(hudLayer))
                throw new HudLayerNotFoundException(typeof(THudLayer));
        }

        hudLayer.Dispose();
        LayerRemoved?.Invoke(this, hudLayer);
    }

    public bool TryRemoveLayer<THudLayer>() where THudLayer : IHudLayer
    {
        IHudLayer? hudLayer;
        lock (_lock)
        {
            hudLayer = _hudLayers.OfType<THudLayer>().FirstOrDefault();

            if (hudLayer == null || !_hudLayers.Remove(hudLayer))
                return false;
        }

        hudLayer.Dispose();
        LayerRemoved?.Invoke(this, hudLayer);

        return true;
    }
    
    public bool TryRemoveLayer(IHudLayer hudLayer)
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
    
    public bool TryGetLayer<THudLayer>(out THudLayer layer) where THudLayer : IHudLayer
    {
        THudLayer? hudLayer;
        lock (_lock)
        {
            hudLayer = _hudLayers.OfType<THudLayer>().FirstOrDefault();
        }

        if (hudLayer != null)
        {
            layer = hudLayer;
            return true;
        }
        layer = default!;
        return false;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            IHudLayer[] view;
            lock (_lock)
                view = [.. _hudLayers];

            foreach (var layer in view)
            {
                RemoveLayer(layer);
            }
        }
    }

    public IEnumerator<IHudLayer> GetEnumerator()
    {
        IHudLayer[] view;
        lock (_lock)
            view = [.. _hudLayers];

        foreach (var layer in view)
        {
            yield return layer;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
