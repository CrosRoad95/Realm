using RealmCore.Server.Concepts.Gui;

namespace RealmCore.Sample.Concepts.Gui;

public sealed class BuyVehicleGui : ReactiveDxGui<BuyVehicleGui.VehicleBuyState>
{
    public class VehicleBuyState
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public Func<Task>? Bought { get; set; }

    public BuyVehicleGui(RealmPlayer player, string name, decimal price) : base(player, "buyVehicle", false, new VehicleBuyState
    {
        Name = name,
        Price = (double)price,
    })
    {
    }

    public async Task HandleForm(IFormContext formContext)
    {
        switch (formContext.FormName)
        {
            case "buy":
                if (Bought != null)
                    await Bought();
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
