﻿using System.Drawing;

namespace RealmCore.Resources.Admin.Data;

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
