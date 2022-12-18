using Realm.Resources.AdminTools.Data;
using System.Numerics;

namespace Realm.Resources.AdminTools.Interfaces;

public interface IWorldDebugData
{
    DebugData DebugData { get; }
    Vector3 Position { get; }
}
