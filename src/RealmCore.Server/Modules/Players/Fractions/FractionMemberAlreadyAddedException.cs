namespace RealmCore.Server.Modules.Players.Fractions;

public class FractionMemberAlreadyAddedException : FractionMemberException
{
    public int UserId { get; }
    public FractionMemberAlreadyAddedException(int userId, int fractionId) : base(fractionId)
    {
        UserId = userId;
    }

    public override string ToString()
    {
        return $"User of id {UserId} is already added to fraction of id {FractionId}";
    }
}
