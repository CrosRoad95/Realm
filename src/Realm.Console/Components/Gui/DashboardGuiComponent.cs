namespace Realm.Console.Components.Gui;

public sealed class DashboardGuiComponent : StatefulGuiComponent<DashboardGuiComponent.DashboardState>
{
    public class DashboardState
    {
        public string A { get; set; } = "A";
        public string B { get; set; } = "B";
        public string C { get; set; } = "C";
        public int Counter { get; set; }
    }

    public DashboardGuiComponent() : base("dashboard", false, new DashboardState())
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        switch(actionContext.ActionName)
        {
            case "counter":
                ChangeState(x => x.Counter, GetStateValue(x => x.Counter) + 1);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
