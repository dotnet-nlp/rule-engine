﻿using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;

// todo [realtime performance] figure out how to introduce single instance for empty immutable result
public sealed class RuleMatchResultCollection : HashSet<RuleMatchResult>
{
    public RuleMatchResultCollection(int capacity) : base(capacity, RuleMatchResultEqualityComparer.Instance)
    {
    }

    public RuleMatchResultCollection(IEnumerable<RuleMatchResult> results) : base(results, RuleMatchResultEqualityComparer.Instance)
    {
    }

    public RuleMatchResultCollection(IEnumerable<RuleMatchResult> results, int capacity) : base(capacity, RuleMatchResultEqualityComparer.Instance)
    {
        UnionWith(results);
    }

    public RuleMatchResult? Best(IResultSelectionStrategy strategy)
    {
        return Count == 0
            ? default
            : this.Aggregate((maxItem, nextItem) => strategy.Compare(maxItem, nextItem) < 0 ? nextItem : maxItem);
    }

    public RuleMatchResultCollection GetFullMatches()
    {
        var fullMatches = new RuleMatchResultCollection(
            this.Where(result => result.LastUsedSymbolIndex == result.Source.Count - 1),
            Count
        );

        fullMatches.TrimExcess();

        return fullMatches;
    }

    public RuleMatchResultCollection ExcludeEmptyMatches()
    {
        var notEmptyMatches = new RuleMatchResultCollection(
            this.Where(result => result.LastUsedSymbolIndex != -1),
            Count
        );

        notEmptyMatches.TrimExcess();

        return notEmptyMatches;
    }

    private class RuleMatchResultEqualityComparer : IEqualityComparer<RuleMatchResult>
    {
        public static readonly RuleMatchResultEqualityComparer Instance = new();

        private RuleMatchResultEqualityComparer()
        {
        }

        public bool Equals(RuleMatchResult? x, RuleMatchResult? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.LastUsedSymbolIndex == y.LastUsedSymbolIndex &&
                   x.ExplicitlyMatchedSymbolsCount == y.ExplicitlyMatchedSymbolsCount &&
                   DictionaryEqualityComparer<string, object?>.Instance.Equals(x.CapturedVariables, y.CapturedVariables);
        }

        public int GetHashCode(RuleMatchResult obj)
        {
            var hashCode = obj.CapturedVariables is not null
                ? DictionaryEqualityComparer<string, object?>.Instance.GetHashCode(obj.CapturedVariables)
                : 0;
            hashCode = (hashCode * 397) ^ obj.ExplicitlyMatchedSymbolsCount.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.LastUsedSymbolIndex.GetHashCode();
            return hashCode;
        }

        private class DictionaryEqualityComparer<TKey, TValue> : IEqualityComparer<IReadOnlyDictionary<TKey, TValue>>
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
                return obj.GetHashCode();
            }
        }
    }
}

public static class RuleMatchResultCollectionExtensions
{
    public static RuleMatchResultCollection Merge(this IReadOnlyList<RuleMatchResultCollection> sources)
    {
        var result = new RuleMatchResultCollection(
            sources.Aggregate(0, (count, collection) => count + collection.Count)
        );

        foreach (var collection in sources)
        {
            foreach (var newResult in collection)
            {
                result.Add(newResult);
            }
        }

        return result;
    }
}