namespace RealmCore.BlazorHelpers;

public static class RazorComponentsEndpointConventionBuilderExtensions
{
    public static RazorComponentsEndpointConventionBuilder AddRealmBlazor(this RazorComponentsEndpointConventionBuilder razorComponentsEndpointConventionBuilder)
    {
        razorComponentsEndpointConventionBuilder.AddAdditionalAssemblies(typeof(RealmGuiIndex).Assembly);
        return razorComponentsEndpointConventionBuilder;
    }
}
