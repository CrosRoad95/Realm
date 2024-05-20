
namespace RealmCore.Server.Modules.Security;

public enum KnownAntiCheatViolation
{
    ExplosionSpam,
}

public record AntiCheatViolationDetails();

public interface IAntiCheat
{
    void ReportViolation(RealmPlayer player, KnownAntiCheatViolation violation, AntiCheatViolationDetails? details);
}

internal sealed class AntiCheat : IAntiCheat
{
    private readonly ILogger<AntiCheat> _logger;

    public event Action<RealmPlayer, int, AntiCheatViolationDetails?>? ViolationReported;
    public AntiCheat(ILogger<AntiCheat> logger)
    {
        _logger = logger;
    }

    public void ReportViolation(RealmPlayer player, KnownAntiCheatViolation violation, AntiCheatViolationDetails? details)
    {
        ViolationReported?.Invoke(player, (int)violation, details);
    }

    public void ReportViolation(RealmPlayer player, int violation, AntiCheatViolationDetails? details)
    {
        ViolationReported?.Invoke(player, violation, details);
    }
}
