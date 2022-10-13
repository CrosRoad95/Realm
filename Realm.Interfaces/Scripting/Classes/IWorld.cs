namespace Realm.Interfaces.Scripting.Classes;

[Name("World")]
public interface IWorld
{
    ISpawn CreateSpawn(string id, string name, Vector3 position, Vector3? rotation = null);
    IElement? GetElementByTypeAndId(string type, string id);
    object GetElementsByType(string type);
}