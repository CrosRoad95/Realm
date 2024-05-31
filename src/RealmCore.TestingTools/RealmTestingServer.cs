using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;

namespace RealmCore.TestingTools;

public class PAttach120DelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if(request.RequestUri?.ToString() == "https://github.com/Patrick2562/mtasa-pAttach/releases/download/v1.2.0/pAttach-v1.2.0.zip")
        {
            string content = "<meta><script src=\"client.lua\" type=\"client\" /><export function=\"attach\" type=\"shared\" /></meta>";

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {

                {
                    var entry = archive.CreateEntry("pAttach/meta.xml");
                    using var writer = new StreamWriter(entry.Open());
                    await writer.WriteAsync(content);
                }

                {
                    var entry = archive.CreateEntry("pAttach/client.lua");
                    using var writer = new StreamWriter(entry.Open());
                    await writer.WriteAsync(Guid.NewGuid().ToString()); // Write random data to invalidate potential cache
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            var data = memoryStream.ToArray();
            response.Content = new ByteArrayContent(memoryStream.ToArray());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "archive.zip"
            };

            return response;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

internal class TestResourceProvider : IResourceProvider
{
    public int Resources = 0;
    public TestResourceProvider()
    {
    }

    public void AddResourceInterpreter(IResourceInterpreter resourceInterpreter)
    {
        throw new NotImplementedException();
    }

    public byte[] GetFileContent(string resource, string file)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetFilesForResource(string name)
    {
        throw new NotImplementedException();
    }

    public Resource GetResource(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Resource> GetResources()
    {
        yield break;
    }

    public void Refresh()
    {
    }

    public ushort ReserveNetId()
    {
        return 420;
    }
}
