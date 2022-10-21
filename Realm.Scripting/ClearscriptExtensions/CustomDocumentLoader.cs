namespace Realm.Scripting.ClearscriptExtensions;

internal class CustomDocumentLoader : DocumentLoader
{
    private readonly string _basePath;

    public CustomDocumentLoader(string? basePath = null)
    {
        _basePath = basePath ?? "";
    }

    public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
    {
        var file = await File.ReadAllTextAsync(Path.Join(_basePath, "Server", specifier));
        return new StringDocument(new DocumentInfo(specifier)
        {
            Category = category,
            ContextCallback = contextCallback
        }, file);
    }
}
