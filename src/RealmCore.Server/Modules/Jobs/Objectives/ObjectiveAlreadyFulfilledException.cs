namespace RealmCore.Server.Modules.Jobs.Objectives;

public class ObjectiveAlreadyFulfilledException : Exception
{
    public ObjectiveAlreadyFulfilledException()
        : base("Objective has already been fulfilled.")
    {
    }
}