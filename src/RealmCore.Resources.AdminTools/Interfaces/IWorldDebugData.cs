using RealmCore.Resources.AdminTools.Data;
using System.Numerics;

namespace RealmCore.Resources.AdminTools.Interfaces;

public interface IWorldDebugData
{
    DebugData DebugData { get; }
    Vector3 Position { get; }
}
