using Realm.Discord;
using Realm.Interfaces.Discord;
using Realm.Interfaces.Extend;
using Realm.Scripting;
using Realm.Server;
using Realm.Server.Interfaces;

string basePath;
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    basePath = ".";
else
    basePath = Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)[6..];

var subscribableLogsSink = new SubscribableLogsSink();
var logger = new Logger()
    .ByExcluding<IDiscord>()
    .WithSink(subscribableLogsSink).GetLogger();
var builder = WebApplication.CreateBuilder(args);
Realm.Configuration.ConfigurationProvider.AddRealmConfiguration(builder.Configuration, basePath);
builder.Logging.ClearProviders();
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.AddTransient<SettingsService>();
builder.Services.AddSingleton<ConsoleService>();
builder.Services.AddTransient<JSRuntimeService>();

builder.Services.AddSingleton(subscribableLogsSink);
builder.Services.AddSingleton<SnackbarFunctions>();
builder.Services.AddSingleton<WebPanelIntegration>();
builder.Services.AddSingleton<WebPanelModule>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton(x => new MTARPGServerImpl(x.GetRequiredService<ConsoleService>(), logger, new Realm.Configuration.ConfigurationProvider(x.GetRequiredService<IConfiguration>()), new IModule[]
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
var seedFileNames = serverImpl.ConfigurationProvider.Get<string[]>("General:SeedFiles");
await serverImpl.BuildFromSeedFiles(seedFileNames);
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
