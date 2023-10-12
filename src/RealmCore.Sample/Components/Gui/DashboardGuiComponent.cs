using RealmCore.Server.Contexts.Interfaces;
using RealmCore.Server.DTOs;

namespace RealmCore.Sample.Components.Gui;

[ComponentUsage(false)]
public sealed class DashboardGuiComponent : StatefulDxGuiComponent<DashboardGuiComponent.DashboardState>
{
    public class DashboardState
    {
        public int Counter { get; set; }
        public double Money { get; set; }
        public List<VehicleLightInfoDTO> VehicleLightInfos { get; set; }
    }

    public DashboardGuiComponent(DashboardState state) : base("dashboard", false, state)
    {

    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        switch (actionContext.ActionName)
        {
            case "increase":
                ChangeState(x => x.Counter, GetStateValue(x => x.Counter) + 1);
                break;
            case "decrease":
                ChangeState(x => x.Counter, state => state.Counter - 1);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
