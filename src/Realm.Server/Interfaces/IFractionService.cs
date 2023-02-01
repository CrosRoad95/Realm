namespace Realm.Server.Interfaces;

public interface IFractionService
{
    void CreateFraction(int id, string fractionName, string fractionCode, Vector3 position);
}
