namespace Realm.MTARPGServer;

internal class Provisioning
{
    public class Spawn
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
    }

    public Dictionary<string, Spawn> Spawns = new();
}


internal class ProvisioningValidator : AbstractValidator<Provisioning>
{
    public ProvisioningValidator()
    {

    }
}