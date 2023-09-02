using RealmCore.BlazorGui;
using RealmCore.Sample;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.MapRazorComponents<App>();

var _ = Task.Run(async () =>
{
    try
    {
        await new SampleServer().Start();
    }
    catch(Exception ex)
    {
        Console.Write(ex.ToString());
    }
});

app.Run();