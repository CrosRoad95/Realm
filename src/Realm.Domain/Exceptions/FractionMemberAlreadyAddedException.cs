namespace Realm.Domain.Exceptions;

public class FractionMemberAlreadyAddedException : Exception
{
    public FractionMemberAlreadyAddedException(int userId, int fractionId) : base($"User of id {userId} is already added to fraction of id {fractionId}") { }
}
