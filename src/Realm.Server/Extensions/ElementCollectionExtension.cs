using SlipeServer.Server.ElementCollections;

namespace Realm.Server.Extensions;

internal static class ElementCollectionExtension
{
    public static void TryDestroyAndDispose(this IElementCollection elementCollection, Element element)
    {
        if(elementCollection.IsElement(element))
        {
            element.Destroy();
            elementCollection.Remove(element);
            if (element is IDisposable disposableElement)
                disposableElement.Dispose();
        }
    }

    public static bool IsElement(this IElementCollection elementCollection, Element element)
    {
        if (elementCollection.Get(element.Id) != null)
            return true;
        return elementCollection.GetAll().Any(x => x == element);
    }
}
