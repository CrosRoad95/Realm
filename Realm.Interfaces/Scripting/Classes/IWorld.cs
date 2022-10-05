namespace Realm.Interfaces.Scripting.Classes;

public interface IWorld
{
    ISpawn CreateSpawn(string name, Vector3 position, Vector3? rotation = null);
}