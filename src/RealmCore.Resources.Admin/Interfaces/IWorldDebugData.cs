using RealmCore.Resources.Admin.Data;
using System.Numerics;

namespace RealmCore.Resources.Admin.Interfaces;

public interface IWorldDebugData
{
    DebugData DebugData { get; }
    Vector3 Position { get; }
}
