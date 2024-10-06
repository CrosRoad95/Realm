namespace RealmCore.Server.Modules.Players.Gui.Dx;

public sealed class PlayerHudFeature : IPlayerFeature, IEnumerable<IHudLayer>, IDisposable
{
    private readonly object _lock = new();

    private readonly List<IHudLayer> _hudLayers = [];
    private readonly IOverlayService _overlayService;

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

    public PlayerHudFeature(PlayerContext playerContext, IOverlayService overlayService)
    {
        Player = playerContext.Player;
        _overlayService = overlayService;
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

    public ClientBlip CreateBlip(int icon, Vector3 position, Color color, float visibleDistance, float size, int interior, int dimension)
    {
        var id = Guid.NewGuid().ToString();

        _overlayService.AddBlip(Player, id, icon, position, color, visibleDistance, size, interior, dimension);
        return new(id);
    }

    public void RemoveBlip(ClientBlip clientBlip)
    {
        _overlayService.RemoveBlip(Player, clientBlip.id);
    }

    public void RemoveAllBlips()
    {
        _overlayService.RemoveAllBlips(Player);
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
                TryRemoveLayer(layer);
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

public readonly record struct ClientBlip(string id);