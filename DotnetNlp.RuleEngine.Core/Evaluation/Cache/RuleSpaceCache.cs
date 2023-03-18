using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Cache;

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
        bool isProjected,
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        KeyValuePair<string, object?>[]? ruleArguments,
        IReadOnlyDictionary<string, object?>? ruleDependenciesOnRuleSpaceArguments
    )
    {
        _results.TryGetValue(
            new Key(
                isProjected,
                ruleId,
                inputSequence,
                nextSymbolIndex,
                ruleArguments,
                ruleDependenciesOnRuleSpaceArguments
            ),
            out var result
        );

        return result;
    }

    public void SetResult(
        bool isProjected,
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        KeyValuePair<string, object?>[]? ruleArguments,
        IReadOnlyDictionary<string, object?>? ruleDependenciesOnRuleSpaceArguments,
        RuleMatchResultCollection result
    )
    {
        try
        {
            _results.Add(
                new Key(
                    isProjected,
                    ruleId,
                    inputSequence,
                    nextSymbolIndex,
                    ruleArguments,
                    ruleDependenciesOnRuleSpaceArguments
                ),
                result
            );
        }
        catch (Exception exception)
        {
            throw new RuleMatchException("Error during adding rule match result to cache.", exception);
        }
    }

    private readonly struct Key : IEquatable<Key>
    {
        private readonly bool _isProjected;
        private readonly int _ruleId;
        private readonly string[] _inputSequence;
        private readonly int _nextSymbolIndex;
        private readonly KeyValuePair<string, object?>[]? _ruleArguments;
        private readonly IReadOnlyDictionary<string, object?>? _ruleDependenciesOnRuleSpaceArguments;

        public Key(bool isProjected,
            int ruleId,
            string[] inputSequence,
            int nextSymbolIndex,
            KeyValuePair<string, object?>[]? ruleArguments,
            IReadOnlyDictionary<string, object?>? ruleDependenciesOnRuleSpaceArguments
        )
        {
            _isProjected = isProjected;
            _ruleId = ruleId;
            _inputSequence = inputSequence;
            _nextSymbolIndex = nextSymbolIndex;
            _ruleArguments = ruleArguments;
            _ruleDependenciesOnRuleSpaceArguments = ruleDependenciesOnRuleSpaceArguments;
        }

        public bool Equals(Key other)
        {
            return _isProjected == other._isProjected &&
                   _ruleId == other._ruleId &&
                   _nextSymbolIndex == other._nextSymbolIndex &&
                   _inputSequence.SequenceEqual(other._inputSequence) &&
                   _ruleArguments is null == other._ruleArguments is null &&
                   (_ruleArguments?.SequenceEqual(other._ruleArguments!) ?? true) &&
                   DictionaryEqualityComparer<string, object?>.Instance.Equals(
                       _ruleDependenciesOnRuleSpaceArguments,
                       other._ruleDependenciesOnRuleSpaceArguments
                   );
        }

        public override bool Equals(object? obj)
        {
            return obj is Key other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = 21;
            hash = hash * 17 + _isProjected.GetHashCode();
            hash = hash * 17 + _ruleId.GetHashCode();
            hash = hash * 17 + _nextSymbolIndex.GetHashCode();
            hash = hash * 17 + _inputSequence.GetSequenceHashCode<string>();
            hash = hash * 17 + (_ruleArguments?.GetSequenceHashCode<KeyValuePair<string, object?>>() ?? 0);
            hash = hash * 17 + (
                _ruleDependenciesOnRuleSpaceArguments is null
                    ? 0
                    : DictionaryEqualityComparer<string, object?>.Instance.GetHashCode(
                        _ruleDependenciesOnRuleSpaceArguments
                    )
            );
            return hash;
        }
    }
}