namespace RealmCore.Server.Modules.Players.Fractions;

public abstract class FractionMemberException : Exception
{
    public int FractionId { get; }

    public FractionMemberException(int fractionId)
    {
        FractionId = fractionId;
    }
}
