using Realm.Resources.AdminTools.Enums;
using System.Drawing;
using System.Numerics;

namespace Realm.Resources.AdminTools.Interfaces;

public interface IWorldDebugData
{
    Guid DebugId { get; }
    Vector3 Position { get; }
    PreviewType PreviewType { get; }
    Color PreviewColor { get; }
}
