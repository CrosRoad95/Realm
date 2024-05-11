using RealmCore.BlazorGui;
using RealmCore.BlazorHelpers;
using RealmCore.Discord.Integration.Extensions;
using RealmCore.Module.Discord;
using RealmCore.Module.Discord.Services;
using RealmCore.Sample;
using SlipeServer.Server;
using System.Reflection;

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

configuration.AddUserSecrets(Assembly.GetEntryAssembly()!);
configuration.AddJsonFile("appsettings.server.json", false, true);
configuration.AddJsonFile("appsettings.development.json", true, true); // TODO: don't use hardcoded development
configuration.AddJsonFile("appsettings.local.json", true, true);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRealmServer(x => new SampleServer(x.GetRequiredService<IConfiguration>()));
builder.AddRealmBlazorGuiSupport();
builder.AddRealmServerDiscordBotIntegration();
builder.Services.AddSingleton<Func<RealmDiscordService>>(x => () => x.GetRequiredService<MtaServer>().GetRequiredService<RealmDiscordService>());
builder.Services.AddDiscordStatusChannelUpdateHandler<SampleDiscordStatusChannelUpdateHandler>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddRealmBlazor()
    .AddInteractiveServerRenderMode();

app.Map("asd", (int a, int b, int c) =>
{

});
await app.RunAsync();
