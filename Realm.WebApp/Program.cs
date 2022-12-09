using Realm.Discord;
using Realm.Interfaces.Extend;
using Realm.Scripting;
using Realm.Server.Interfaces;
using Realm.Server.Modules;

var subscribableLogsSink = new SubscribableLogsSink();
var logger = new RealmLogger()
    .WithSink(subscribableLogsSink).GetLogger();
var builder = WebApplication.CreateBuilder(args);
Realm.Configuration.RealmConfigurationProvider.AddRealmConfiguration(builder.Configuration);
builder.Logging.ClearProviders();
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.AddTransient<SettingsService>();
builder.Services.AddSingleton<ConsoleService>();
builder.Services.AddTransient<JSRuntimeService>();

builder.Services.AddSingleton(subscribableLogsSink);
builder.Services.AddSingleton<SnackbarScriptingFunctions>();
builder.Services.AddSingleton<WebPanelIntegration>();
builder.Services.AddSingleton<WebPanelModule>();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton(x => new MTARPGServerImpl(x.GetRequiredService<ConsoleService>(), logger, new Realm.Configuration.RealmConfigurationProvider(x.GetRequiredService<IConfiguration>()), new IModule[]
        {
            new DiscordModule(),
            new IdentityModule(),
            new ScriptingModule(),
            x.GetRequiredService<WebPanelModule>(),
            new ServerScriptingModule(),
        }));
builder.Services.AddSingleton<IRPGServer>(x => x.GetRequiredService<MTARPGServerImpl>().Server);

var app = builder.Build();

var serverImpl = app.Services.GetRequiredService<MTARPGServerImpl>();

// TODO: make seed files optional
var seedFileNames = serverImpl.ConfigurationProvider.GetRequired<string[]>("General:SeedFiles");
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
