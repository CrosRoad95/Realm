namespace RealmCore.Server.DomainObjects;

public sealed class Bans : IDisposable
{
    private readonly List<BanDTO> _bans = [];
    private readonly object _lock = new();
    public event Action<BanDTO>? BanAdded;
    public event Action<BanDTO>? BanRemoved;
    private bool _disposed;
    public IReadOnlyList<BanDTO> All
    {
        get
        {
            lock (_lock)
                return new List<BanDTO>(_bans);
        }
    }

    internal Bans() { }

    internal Bans(List<BanDTO> bans)
    {
        _bans = bans;
    }

    internal void AddBan(BanDTO ban)
    {
        lock (_lock)
            _bans.Add(ban);

        BanAdded?.Invoke(ban);
    }

    internal void RemoveBan(BanDTO ban)
    {
        bool removed = false;
        lock (_lock)
            removed = _bans.Remove(ban);

        if (removed)
            BanRemoved?.Invoke(ban);
    }

    internal void RemoveBan(int banId)
    {
        BanDTO? removedBan = null;
        lock (_lock)
        {
            var index = _bans.FindIndex(x => x.Id == banId);
            if(index >= 0)
            {
                removedBan = _bans[index];
                _bans.RemoveAt(index);
            }
        }

        if (removedBan != null)
            BanRemoved?.Invoke(removedBan);
    }

    public bool IsBanned(DateTime now, int type)
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            foreach (var banDTO in _bans)
            {
                if (banDTO.Type == type && now < banDTO.End)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryGetBan(DateTime now, int type, out BanDTO? foundBanDTO)
    {
        ThrowIfDisposed();
        lock (_lock)
        {
            foreach (var banDTO in _bans)
            {
                if (banDTO.Type == type && now > banDTO.End)
                {
                    foundBanDTO = banDTO;
                    return true;
                }
            }
        }
        foundBanDTO = null;
        return false;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
