namespace RealmCore.Resources.Overlay.Interfaces;

public interface IHudBuilder
{
    Action<DynamicHudElement>? DynamicHudElementAdded { get; set; }

    IHudBuilder Add(IHudElement hudElement, AddElementLocation addElementLocation = AddElementLocation.Default);
}
