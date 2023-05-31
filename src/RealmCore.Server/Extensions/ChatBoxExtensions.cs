namespace RealmCore.Server.Extensions;

public static class ChatBoxExtensions
{
    public static void OutputTo(this ChatBox chatBox, Entity entity, string message, Color? color = null, bool isColorCoded = false)
    {
        chatBox.OutputTo(entity.Player, message, color ?? Color.White, isColorCoded);
    }

    public static void ClearFor(this ChatBox chatBox, Entity entity)
    {
        chatBox.ClearFor(entity.Player);
    }

    public static void SetVisibleFor(this ChatBox chatBox, Entity entity, bool visible)
    {
        chatBox.SetVisibleFor(entity.Player, visible);
    }
}
