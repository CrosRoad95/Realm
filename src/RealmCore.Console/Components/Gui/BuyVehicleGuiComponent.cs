using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class BuyVehicleGuiComponent : StatefulGuiComponent<BuyVehicleGuiComponent.VehicleBuyState>
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

    protected override async Task HandleAction(IActionContext actionContext)
    {
    }
}
