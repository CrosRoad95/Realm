namespace RealmCore.Resources.Assets;

internal sealed class BuildInFont : IBuiltInFont
{
    public string Name { get; }
    public string Checksum { get; } = string.Empty;

    public BuildInFont(string name)
    {
        Name = name;
    }
}

public static class BuildInFonts
{
    public static IBuiltInFont Default { get; } = new BuildInFont("default");
    public static IBuiltInFont DefaultBold { get; } = new BuildInFont("default-bold");
    public static IBuiltInFont Clear { get; } = new BuildInFont("clear");
    public static IBuiltInFont Arial { get; } = new BuildInFont("arial");
    public static IBuiltInFont Sans { get; } = new BuildInFont("sans");
    public static IBuiltInFont Pricedown { get; } = new BuildInFont("pricedown");
    public static IBuiltInFont Bankgothic { get; } = new BuildInFont("bankgothic");
    public static IBuiltInFont Diploma { get; } = new BuildInFont("diploma");
    public static IBuiltInFont Beckett { get; } = new BuildInFont("beckett");
    public static IBuiltInFont Unifont { get; } = new BuildInFont("unifont");
}
