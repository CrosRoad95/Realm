namespace RealmCore.Server.Modules.Elements;

public sealed class Nametag
{
    public event Action<Nametag, string?>? TextChanged;
    public event Action<Nametag, bool>? ShowMyNametagChanged;

    private readonly Ped _ped;
    private bool _showMyNametag;
    private string? _text;

    public Ped Ped => _ped;

    public string? Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                TextChanged?.Invoke(this, value);
            }
        }
    }

    public bool ShowMyNametag
    {
        get => _showMyNametag; set
        {
            if (_ped is Player)
            {
                if (_showMyNametag != value)
                {
                    _showMyNametag = value;
                    ShowMyNametagChanged?.Invoke(this, value);
                }
            }
        }
    }

    internal Nametag(Ped ped)
    {
        _ped = ped;
    }
}
