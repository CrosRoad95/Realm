using FluentValidation;

namespace Realm.Server.Seeding;

internal class SeedData
{
    public class BlipSeedData
    {
        public int Icon { get; set; }
        public Vector3 Position { get; set; }
    }

    public class PickupSeedData
    {
        public ushort Model { get; set; }
        public Vector3 Position { get; set; }
    }

    public class FractionMemberSeedData
    {
        public string[]? Permissions { get; set; }
    }

    public class FractionSeedData
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Dictionary<string, FractionMemberSeedData> Members { get; set; }
    }

    public class AccountSeedData
    {
        public string Password { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public List<string> Roles { get; set; }
    }

    public class VehicleUpgradeDescriptionSeedData
    {
        public float[] MaxVelocity { get; set; }
        public float[] EngineAcceleration { get; set; }
    }

    public Dictionary<string, BlipSeedData> Blips = new();
    public Dictionary<string, PickupSeedData> Pickups = new();
    public Dictionary<string, FractionSeedData> Fractions = new();
    public List<string> Roles = new();
    public Dictionary<string, AccountSeedData> Accounts = new();
    public Dictionary<string, VehicleUpgradeDescriptionSeedData> Upgrades = new();
}

internal class SeedValidator : AbstractValidator<SeedData>
{
    public SeedValidator()
    {

    }
}