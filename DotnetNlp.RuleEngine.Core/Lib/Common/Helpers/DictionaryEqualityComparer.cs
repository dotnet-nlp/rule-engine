using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

public sealed class DictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<IReadOnlyDictionary<TKey, TValue>>
    where TKey : notnull
{
    public static readonly DictionaryEqualityComparer<TKey, TValue> Instance = new();

    private DictionaryEqualityComparer()
    {
    }

    public bool Equals(IReadOnlyDictionary<TKey, TValue>? x, IReadOnlyDictionary<TKey, TValue>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.Count != y.Count)
        {
            return false;
        }

        foreach (var (xKey, xValue) in x)
        {
            if (!y.TryGetValue(xKey, out var yValue))
            {
                return false;
            }

            if (!Equals(xValue, yValue))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(IReadOnlyDictionary<TKey, TValue> obj)
    {
        return obj.GetDictionaryHashCode();
    }
}