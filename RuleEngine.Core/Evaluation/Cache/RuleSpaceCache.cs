using System;
using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Evaluation.Rule.Result;
using RuleEngine.Core.Exceptions;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Evaluation.Cache;

public sealed class RuleSpaceCache : IManageableRuleSpaceCache
{
    private readonly IDictionary<Key, RuleMatchResultCollection> _results;

    public RuleSpaceCache(int capacity = 0)
    {
        _results = new Dictionary<Key, RuleMatchResultCollection>(capacity);
    }

    public void Clear()
    {
        _results.Clear();
    }

    public RuleMatchResultCollection? GetResult(
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        IReadOnlyDictionary<string, object?>? ruleArguments
    )
    {
        _results.TryGetValue(new Key(ruleId, inputSequence, ruleArguments, nextSymbolIndex), out var result);

        return result;
    }

    public void SetResult(
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        IReadOnlyDictionary<string, object?>? ruleArguments,
        RuleMatchResultCollection result
    )
    {
        try
        {
            _results.Add(new Key(ruleId, inputSequence, ruleArguments, nextSymbolIndex), result);
        }
        catch (Exception exception)
        {
            throw new RuleMatchException("Error during adding rule match result to cache.", exception);
        }
    }

    private readonly struct Key : IEquatable<Key>
    {
        private readonly int _ruleId;
        private readonly string[] _inputSequence;
        private readonly IReadOnlyDictionary<string, object?>? _ruleArguments;
        private readonly int _nextSymbolIndex;

        public Key(
            int ruleId,
            string[] inputSequence,
            IReadOnlyDictionary<string, object?>? ruleArguments,
            int nextSymbolIndex
        )
        {
            _ruleId = ruleId;
            _inputSequence = inputSequence;
            _nextSymbolIndex = nextSymbolIndex;
            _ruleArguments = ruleArguments;
        }

        public bool Equals(Key other)
        {
            return _ruleId == other._ruleId &&
                   _nextSymbolIndex == other._nextSymbolIndex &&
                   _inputSequence.SequenceEqual(other._inputSequence) &&
                   _ruleArguments is null == other._ruleArguments is null &&
                   (_ruleArguments?.SequenceEqual(other._ruleArguments!) ?? true);
        }

        public override bool Equals(object? obj)
        {
            return obj is Key other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 21;
            hash = hash * 17 + _ruleId.GetHashCode();
            hash = hash * 17 + _nextSymbolIndex.GetHashCode();
            hash = hash * 17 + _inputSequence.GetSequenceHashCode<string>();
            hash = hash * 17 + (_ruleArguments?.GetDictionaryHashCode() ?? 0);
            return hash;
        }
    }
}