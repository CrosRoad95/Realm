using RealmCore.Server.Concepts.Gui;
using RealmCore.Server.DTOs;

namespace RealmCore.Sample.Concepts.Gui;

public sealed class DashboardGui : ReactiveDxGui<DashboardGui.DashboardState>, IGuiHandlers
{
    public class DashboardState
    {
        public int Counter { get; set; }
        public double Money { get; set; }
        public List<VehicleLightInfoDTO> VehicleLightInfos { get; set; }
    }

    public DashboardGui(RealmPlayer realmPlayer, DashboardState state) : base(realmPlayer, "dashboard", false, state)
    {

    }

    public async Task HandleAction(IActionContext actionContext)
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

    public Task HandleForm(IFormContext formContext)
    {
        throw new NotImplementedException();
    }
}
