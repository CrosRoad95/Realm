using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.ClearProviders();
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

builder.Services.AddTransient<SettingsService>();
builder.Services.AddTransient<JSRuntimeService>();
builder.Services.AddSingleton(GrpcChannel.ForAddress("http://localhost:22010"));

builder.Services.AddSingleton<ServerService>();
builder.Services.AddSingleton<SettingsService>();

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
