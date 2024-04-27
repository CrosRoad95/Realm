using RealmCore.BlazorGui;
using RealmCore.BlazorHelpers;
using RealmCore.Discord.Integration.Extensions;
using RealmCore.Module.Discord;
using RealmCore.Sample;

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRealmServer(new SampleServer());
builder.AddRealmBlazorGuiSupport();
builder.AddRealmServerDiscordBotIntegration();
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

await app.RunAsync();
