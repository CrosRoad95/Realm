namespace Realm.Scripting.ClearscriptExtensions;

internal class CustomDocumentLoader : DocumentLoader
{
    public override async Task<Document> LoadDocumentAsync(DocumentSettings settings, DocumentInfo? sourceInfo, string specifier, DocumentCategory category, DocumentContextCallback contextCallback)
    {
        var file = await File.ReadAllTextAsync(Path.Join("Server", specifier));
        return new StringDocument(new DocumentInfo(specifier)
        {
            Category = category,
            ContextCallback = contextCallback
        }, file);
    }
}
