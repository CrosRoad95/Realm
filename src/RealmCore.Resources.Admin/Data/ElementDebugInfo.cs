using System.Drawing;
using System.Numerics;

namespace RealmCore.Resources.Admin.Data;

public struct ElementDebugInfo
{
    public Element? element;
    public Vector3 position;
    public string name;
    public string debugId;
    public PreviewType previewType;
    public Color previewColor;
}
