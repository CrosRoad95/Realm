namespace RealmCore.Persistance.Data.Helpers;

public class VehicleDamageState
{
    public enum PanelState
    {
        Undamaged,
        Damaged,
        Damaged2,
        VeryDamaged,
    }

    public enum DoorState
    {
        ShutIntact,
        AjarIntact,
        ShutDamanged,
        AjarDamaged,
        Missing,
    }

    public enum LightState : byte
    {
        Normal,
        Broken,
    }

    public PanelState FrontLeftPanel { get; set; }
    public PanelState FrontRightPanel { get; set; }
    public PanelState RearLeftPanel { get; set; }
    public PanelState RearRightPanel { get; set; }
    public PanelState Windscreen { get; set; }
    public PanelState FrontBumper { get; set; }
    public PanelState RearBumper { get; set; }

    public DoorState Hood { get; set; }
    public DoorState Trunk { get; set; }
    public DoorState FrontLeftDoor { get; set; }
    public DoorState FrontRightDoor { get; set; }
    public DoorState RearLeftDoor { get; set; }
    public DoorState RearRightDoor { get; set; }

    public LightState FrontLeftLight { get; set; }
    public LightState FrontRightLight { get; set; }
    public LightState RearRightLight { get; set; }
    public LightState RearLeftLight { get; set; }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Formatting.None);
    }

    public static VehicleDamageState CreateFromString(string json)
    {
        return JsonConvert.DeserializeObject<VehicleDamageState>(json) ?? throw new Exception("Failed to create VehicleDamageState from string json");
    }
}
