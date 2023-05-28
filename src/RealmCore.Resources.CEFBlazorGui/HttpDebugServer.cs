using Microsoft.Extensions.Logging;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;

namespace RealmCore.Resources.CEFBlazorGui;

internal class HttpDebugServer
{
    private readonly HttpListener _httpListener;
    private readonly ILogger<HttpDebugServer> _logger;
    private readonly IElementCollection _elementCollection;

    public Action<Player, string, string>? InvokeVoidAsyncHandler { get; set; }

    public HttpDebugServer(ILogger<HttpDebugServer> logger, IElementCollection elementCollection)
    {
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add($"http://*:22100/");
        _logger = logger;
        _elementCollection = elementCollection;
    }

    public void Start()
    {
        try
        {
            _httpListener.Start();
        }
        catch (HttpListenerException ex)
        {
            _logger.LogError("Failed to start http debug server.");
            return;
        }

        Task.Run(async () =>
        {
            while (true)
            {
                var context = await _httpListener.GetContextAsync();
                _ = Task.Run(() => HandleRequest(context));
            }
        });
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        switch(context.Request.HttpMethod)
        {
            case "OPTIONS":
                context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT");
                context.Response.AddHeader("Access-Control-Max-Age", "1728000");
                context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                break;
            case "POST":
                context.Response.ContentType = "text/plain";

                var reader = new StreamReader(context.Request.InputStream);
                var httpRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<CEFHttpRequest>(reader.ReadToEnd());
                switch (httpRequest.Kind)
                {
                    case "invokeVoidAsync":
                        foreach (var player in _elementCollection.GetByType<Player>())
                            InvokeVoidAsyncHandler?.Invoke(player, httpRequest.CSharpIdentifier, httpRequest.Args);
                        break;
                }

                //var foo = System.Text.Encoding.UTF8.GetBytes("asdasd");
                //context.Response.OutputStream.Write(foo);
                break;
        }
        context.Response.StatusCode = 200;
        context.Response.Close();

    }
}
