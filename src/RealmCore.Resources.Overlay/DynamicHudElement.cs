﻿namespace RealmCore.Resources.Overlay;

public class DynamicHudElement
{
    public int Id { get; set; }
    public Delegate Factory { get; set; }
}
