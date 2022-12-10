using Realm.Interfaces.Providers;

namespace Realm.Module.Scripting.ClearscriptExtensions;

internal class CustomDocumentLoader : DocumentLoader
{
    private readonly IServerFilesProvider _serverFilesProvider;

    public CustomDocumentLoader(IServerFilesProvider serverFilesProvider)
    {
        _serverFilesProvider = serverFilesProvider;
    }

    public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
    {
        var file = await _serverFilesProvider.ReadAllText(Path.Join("Scripts", specifier));
        return new StringDocument(new DocumentInfo(specifier)
        {
            Category = category,
            ContextCallback = contextCallback
        }, file);
    }
}
