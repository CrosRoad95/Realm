namespace RealmCore.Resources.MapNames;

public record MapName(string Name, Color Color, Vector3 Position, ushort Dimension = 0, byte Interior = 0, Element? AttachedTo = null, int Category = 0);