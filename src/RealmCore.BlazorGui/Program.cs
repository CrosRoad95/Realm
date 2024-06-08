using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()!.Location)!);

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

var configuration = builder.Configuration;

configuration.AddUserSecrets(Assembly.GetEntryAssembly()!);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

builder.AddRealmServer<RealmPlayer>();

builder.AddRealmBlazorGuiSupport();
builder.AddRealmServerDiscordBotIntegration();
builder.Services.AddSingleton<RealmDiscordService>();
builder.Services.AddSampleServer();
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
