using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Realm.Logging;
using Realm.MTARPGServer;
using Realm.WebApp;
using Realm.WebApp.Data;

var serverConsole = new EmptyConsoleCommands();
var logger = new Logger().GetLogger();
var _configurationProvider = new Realm.Configuration.ConfigurationProvider();
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var path = System.IO.Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)[6..];

var previousDirectory = Directory.GetCurrentDirectory();
Directory.SetCurrentDirectory(path);
var server = new MTARPGServerImpl(serverConsole, logger, _configurationProvider, path);
server.Start();
Directory.SetCurrentDirectory(previousDirectory);

builder.Services.AddSingleton(server.Server);
builder.Services.AddSingleton<WeatherForecastService>();

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

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
