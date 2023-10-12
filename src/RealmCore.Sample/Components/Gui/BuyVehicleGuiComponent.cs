using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Sample.Components.Gui;

[ComponentUsage(false)]
public sealed class BuyVehicleGuiComponent : StatefulDxGuiComponent<BuyVehicleGuiComponent.VehicleBuyState>
{
    public class VehicleBuyState
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public Func<Task>? Bought { get; set; }

    public BuyVehicleGuiComponent(string name, decimal price) : base("buyVehicle", false, new VehicleBuyState
    {
        Name = name,
        Price = (double)price,
    })
    {
    }

    protected override async Task HandleForm(IFormContext formContext)
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
