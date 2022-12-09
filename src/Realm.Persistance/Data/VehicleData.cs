public sealed class VehicleData
{
#pragma warning disable CS8618
    public string VehicleId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
#pragma warning restore CS8618

    public Vehicle? Vehicle { get; set; }
}

