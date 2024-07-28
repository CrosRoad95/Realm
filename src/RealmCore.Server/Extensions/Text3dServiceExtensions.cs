namespace RealmCore.Server.Extensions;

public static class Text3dServiceExtensions
{
    public static int CreateFor(this Text3dService text3dService, Element element, string text, float fontSize = 1f, float distance = 64f, Color? color = null, Vector3 offset = default, Vector2? shadow = null)
    {
        var text3dId = text3dService.CreateText3d(element.Position + offset, text, fontSize, distance, color, element.Interior, element.Dimension, shadow);
        element.Destroyed += (e) =>
        {
            text3dService.RemoveText3d(text3dId);
        };

        return text3dId;
    }
}
