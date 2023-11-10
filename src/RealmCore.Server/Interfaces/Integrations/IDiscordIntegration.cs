
namespace RealmCore.Server.Interfaces.Integrations;

public interface IDiscordIntegration : IIntegration
{
    ulong DiscordUserId { get; internal set; }

    string GenerateAndGetConnectionCode(TimeSpan? validFor = null);
    bool Verify(string code, ulong discordUserId);
}
