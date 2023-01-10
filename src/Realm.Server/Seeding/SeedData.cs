using FluentValidation;
using System.Drawing;

namespace Realm.Server.Seeding;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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
        public string? Text3d { get; set; }
    }

    public class MarkerSeedData
    {
        public Vector3 Position { get; set; }
        public float Size { get; set; } = 1;
        public Color Color { get; set; }
        public MarkerType MarkerType { get; set; } = MarkerType.Cylinder;
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
    public Dictionary<string, MarkerSeedData> Markers = new();
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

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
