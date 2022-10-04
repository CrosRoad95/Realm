namespace Realm.Scripting.Classes;

public interface IWorld
{
    string CreateSpawn(string name, Vector3 position, Vector3? rotation = null);
}