using RealmCore.ECS;

namespace RealmCore.Server.Extensions;

public static class ChatBoxExtensions
{
    public static void OutputTo(this ChatBox chatBox, Entity entity, string message, Color? color = null, bool isColorCoded = false)
    {
        chatBox.OutputTo(entity.GetPlayer(), message, color ?? Color.White, isColorCoded);
    }

    public static void ClearFor(this ChatBox chatBox, Entity entity)
    {
        chatBox.ClearFor(entity.GetPlayer());
    }

    public static void SetVisibleFor(this ChatBox chatBox, Entity entity, bool visible)
    {
        chatBox.SetVisibleFor(entity.GetPlayer(), visible);
    }
}
