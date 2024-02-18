namespace RealmCore.Server.Extensions;

public static class Text3dServiceExtensions
{
    public static int CreateFor(this Text3dService service, Element element, string text, float fontSize = 1f, float distance = 64f, Color? color = null)
    {
        return service.CreateText3d(element.Position, text, fontSize, distance, color);
    }
}
