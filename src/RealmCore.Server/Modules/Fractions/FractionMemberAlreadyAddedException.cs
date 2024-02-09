namespace RealmCore.Server.Modules.Fractions;

public class FractionMemberAlreadyAddedException : Exception
{
    public FractionMemberAlreadyAddedException(int userId, int fractionId) : base($"User of id {userId} is already added to fraction of id {fractionId}") { }
}
