using RealmCore.BlazorGui;
using RealmCore.BlazorHelpers;
using RealmCore.Sample;
using RealmCore.Server.Interfaces;

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

var builder = WebApplication.CreateBuilder(args);
var sampleServer = new SampleServer();
// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IRealmServer>(sampleServer);
builder.AddRealmBlazorGuiSupport();
builder.Services.AddRazorComponents()
    .AddServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(RealmGuiComponentBase).Assembly)
    .AddServerRenderMode();

var _ = Task.Run(async () =>
{
    try
    {
        await sampleServer.Start();
    }
    catch (Exception ex)
    {
        Console.Write(ex.ToString());
    }
});

app.Run();