using Realm.Domain.Contexts;
using Realm.Domain.Data;

namespace Realm.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class DashboardGuiComponent : StatefulGuiComponent<DashboardGuiComponent.DashboardState>
{
    public class DashboardState
    {
        public double Money { get; set; }
        public List<VehicleLightInfo> VehicleLightInfos { get; set; }
    }

    public DashboardGuiComponent(DashboardState state) : base("dashboard", false, state)
    {

    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        switch (actionContext.ActionName)
        {
            case "counter":

                break;
            default:
                throw new NotImplementedException();
        }
    }
}
