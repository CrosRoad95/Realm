namespace RealmCore.Server.Elements;

public class RealmPed : Ped
{
    private string? _nametagText;
    public event Action<RealmPed, string?>? NametagTextChanged;
    public string? NametagText
    {
        get => _nametagText;
        set
        {
            _nametagText = value;
            NametagTextChanged?.Invoke(this, _nametagText);
        }
    }

    public RealmPed(PedModel model, Vector3 position) : base(model, position)
    {
    }
}
