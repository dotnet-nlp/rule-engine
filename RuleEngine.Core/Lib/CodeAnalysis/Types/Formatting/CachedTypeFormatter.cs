using System;
using System.Collections.Generic;

namespace RuleEngine.Core.Lib.CodeAnalysis.Types.Formatting;

public class CachedTypeFormatter : ITypeFormatter
{
    private readonly ITypeFormatter _typeFormatter;
    private readonly IDictionary<Type, string> _cache;

    public CachedTypeFormatter(ITypeFormatter typeFormatter, int capacity = 0)
    {
        _typeFormatter = typeFormatter;
        _cache = new Dictionary<Type, string>(capacity);
    }

    public string GetStringRepresentation(Type type)
    {
        if (!_cache.TryGetValue(type, out var typeName))
        {
            typeName = _typeFormatter.GetStringRepresentation(type);

            _cache.Add(type, typeName);
        }

        return typeName;
    }
}