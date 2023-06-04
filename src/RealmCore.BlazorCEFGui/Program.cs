var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<EventHub>();
builder.Services.AddScoped<RememberForm>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("localhost:22100") });

await builder.Build().RunAsync();
