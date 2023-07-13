namespace RealmCore.Server;

public static class RealmInternal
{
    public static Element GetElement(ElementComponent elementComponent) => elementComponent.Element;
    public static Player GetPlayer(ElementComponent elementComponent) => (Player)elementComponent.Element;
}
