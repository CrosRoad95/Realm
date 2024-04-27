namespace RealmCore.Discord.Integration;

public sealed class IntegrationHeader
{
    public int Version { get; }
    
    public IntegrationHeader(int version)
    {
        Version = version;
    }
}
