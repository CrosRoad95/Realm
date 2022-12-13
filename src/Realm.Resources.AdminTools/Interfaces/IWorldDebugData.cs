using Realm.Resources.AdminTools.Data;
using Realm.Resources.AdminTools.Enums;
using System.Drawing;
using System.Numerics;

namespace Realm.Resources.AdminTools.Interfaces;

public interface IWorldDebugData
{
    DebugData DebugData { get; }
    Vector3 Position { get; }
}
