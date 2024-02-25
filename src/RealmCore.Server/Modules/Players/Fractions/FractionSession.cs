using RealmCore.Server.Modules.Players.Sessions;

namespace RealmCore.Server.Modules.Players.Fractions;

public class FractionSession : Session
{
    public override string Name => "Frakcja";
    public FractionSession(RealmPlayer player, IDateTimeProvider dateTimeProvider) : base(player, dateTimeProvider)
    {
    }
}
