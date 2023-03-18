using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

public static class CollectionExtensions
{
    public static TCollection? NullIfEmpty<TCollection, TValue>(this TCollection collection)
        where TCollection : class, IReadOnlyCollection<TValue>
    {
        return collection.Count == 0 ? null : collection;
    }
}