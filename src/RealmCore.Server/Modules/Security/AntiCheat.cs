namespace RealmCore.Server.Modules.Security;

public enum KnownAntiCheatViolation
{
    ExplosionSpam,
    ExplosionOutsideRange,
}

public record AntiCheatViolationDetails();

public sealed class AntiCheat
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

    public void ReportViolation(RealmPlayer player, int violation, AntiCheatViolationDetails? details = null)
    {
        ViolationReported?.Invoke(player, violation, details);
    }
}
