using Realm.Resources.AdminTools.Enums;
using System.Drawing;

namespace Realm.Resources.AdminTools.Data;

public struct DebugData
{
    public Guid DebugId { get; } = Guid.NewGuid();
    public PreviewType PreviewType { get; }
    public Color PreviewColor { get; }

    public DebugData(PreviewType previewType, Color previewColor)
    {
        PreviewType = previewType;
        PreviewColor = previewColor;
    }
}
