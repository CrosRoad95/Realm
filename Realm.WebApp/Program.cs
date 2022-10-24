using MudBlazor.Services;

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

builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton(x => new MTARPGServerImpl(serverConsole, logger, new Realm.Configuration.ConfigurationProvider(x.GetRequiredService<IConfiguration>()), basePath));
builder.Services.AddSingleton<IRPGServer>(x => x.GetRequiredService<MTARPGServerImpl>().Server);

var app = builder.Build();

app.Services.GetRequiredService<MTARPGServerImpl>().Start();

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
