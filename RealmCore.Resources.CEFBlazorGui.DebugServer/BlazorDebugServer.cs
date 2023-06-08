using System.Text;
using Newtonsoft.Json;

namespace RealmCore.Resources.CEFBlazorGui.DebugServer;

public class BlazorDebugServer
{
    public Func<string, string, Task>? InvokeVoidAsyncHandler { get; set; }
    public Func<string, string, Task<object>>? InvokeAsyncHandler { get; set; }

    static void Main(string[] args) { }
    public BlazorDebugServer()
    {
    }

    public async Task Start()
    {
        await Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(x =>
                {
                    x.AddCors();
                });
                webBuilder.UseUrls("http://*:22100");
                webBuilder.Configure(app =>
                {
                    app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapPost("/invokeVoidAsync", async context =>
                        {
                            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                            string requestBody = await reader.ReadToEndAsync();
                            var invokeRequest = JsonConvert.DeserializeObject<HttpInvokeRequest>(requestBody);
                            if (invokeRequest == null)
                                return;
                            await InvokeVoidAsyncHandler?.Invoke(invokeRequest.CSharpIdentifier, invokeRequest.Args);
                        });
                        endpoints.MapPost("/invokeAsync", async context =>
                        {
                            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                            string requestBody = await reader.ReadToEndAsync();
                            var invokeRequest = JsonConvert.DeserializeObject<HttpInvokeRequest>(requestBody);
                            if (invokeRequest == null)
                                return;

                            var response = await InvokeAsyncHandler?.Invoke(invokeRequest.CSharpIdentifier, invokeRequest.Args);
                            if (response == null)
                                return;

                            var data = JsonConvert.SerializeObject(response);
                            context.Response.StatusCode = 200;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(data);
                        });
                    });
                });
            })
            .RunConsoleAsync();
            //.StartAsync();
    }
}