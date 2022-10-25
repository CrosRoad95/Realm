using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Realm.Discord;
using Realm.Persistance;
using Realm.Scripting;
using Realm.Server;

var basePath = Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)[6..];

var serverConsole = new EmptyConsoleCommands();
var logger = new Logger().GetLogger();
var builder = WebApplication.CreateBuilder(args);
Realm.Configuration.ConfigurationProvider.AddRealmConfiguration(builder.Configuration, basePath);
builder.Logging.ClearProviders();
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.AddSingleton<SnackbarFunctions>();
builder.Services.AddSingleton<WebPanelIntegration>();
builder.Services.AddSingleton<WebPanelModule>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton(x => new MTARPGServerImpl(serverConsole, logger, new Realm.Configuration.ConfigurationProvider(x.GetRequiredService<IConfiguration>()), new IModule[]
        {
            new DiscordModule(),
            new IdentityModule(),
            new ScriptingModule(),
            x.GetRequiredService<WebPanelModule>(),
            new ServerScriptingModule(),
        }, basePath));
builder.Services.AddSingleton<IRPGServer>(x => x.GetRequiredService<MTARPGServerImpl>().Server);

var app = builder.Build();

var serverImpl = app.Services.GetRequiredService<MTARPGServerImpl>();
var fileName = serverImpl.ConfigurationProvider.Get<string>("General:ProvisioningFile");
await serverImpl.BuildFromProvisioningFile(fileName);
serverImpl.Start();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
