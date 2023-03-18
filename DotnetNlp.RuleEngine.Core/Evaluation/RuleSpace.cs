using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation;

internal sealed class RuleSpace : IRuleSpace
{
    public int Id { get; }
    public string Name { get; }
    private readonly Dictionary<string, IRuleMatcher> _ruleMatchersByName;
    private readonly HashSet<string> _transientRulesKeys;
    private readonly Dictionary<string, Type> _ruleResultTypesByName;
    public IReadOnlyDictionary<string, Type> RuleResultTypesByName => _ruleResultTypesByName;
    public IReadOnlyDictionary<string, Type> RuleSpaceParameterTypesByName { get; }
    public IReadOnlySet<string> TransientRulesKeys => _transientRulesKeys;

    public IRuleMatcher this[string ruleName]
    {
        get
        {
            if (TryGetValue(ruleName, out var ruleMatcher))
            {
                return ruleMatcher;
            }

            throw new RuleMatchException(
                $"Rule space doesn't contain rule '{ruleName}'. " +
                $"Available rules are: {Keys.Select(key => $"'{key}'").JoinToString(", ")}."
            );
        }
        set
        {
            _ruleMatchersByName.Add(ruleName, value);
            _ruleResultTypesByName.Add(ruleName, value.ResultDescription.ResultType);
        }
    }

    public RuleSpace(
        int id,
        string name,
        Dictionary<string, Type> ruleResultTypesByName,
        Dictionary<string, IRuleMatcher> ruleMatchersByName,
        HashSet<string> transientRulesKeys,
        IReadOnlyDictionary<string, Type> ruleSpaceParameterTypesByName
    )
    {
        Id = id;
        Name = name;
        _ruleResultTypesByName = ruleResultTypesByName;
        _ruleMatchersByName = ruleMatchersByName;
        _transientRulesKeys = transientRulesKeys;
        RuleSpaceParameterTypesByName = ruleSpaceParameterTypesByName;
    }

    public IReadOnlySet<string> Prune(IReadOnlySet<string> rulesToPreserve)
    {
        var visitedRules = new HashSet<string>();
        var rulesToPreserveWithDependencies = new List<string>();

        WaltThroughDependencyGraph(rulesToPreserve);

        void WaltThroughDependencyGraph(IReadOnlySet<string> rules)
        {
            foreach (var rule in rules)
            {
                if (!visitedRules.Contains(rule))
                {
                    visitedRules.Add(rule);
                    rulesToPreserveWithDependencies.Add(rule);

                    if (TryGetValue(rule, out var ruleMatcher))
                    {
                        WaltThroughDependencyGraph(ruleMatcher.GetDependencies(this));
                    }
                }
            }
        }

        var removedRules = new HashSet<string>();

        foreach (var pair in this.ExceptBy(rulesToPreserveWithDependencies, pair => pair.Key).ToArray())
        {
            Remove(pair);
            removedRules.Add(pair.Key);
        }

        return removedRules;
    }

    public int Count => _ruleMatchersByName.Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<string, IRuleMatcher>>) _ruleMatchersByName).IsReadOnly;
    public ICollection<string> Keys => _ruleMatchersByName.Keys;
    public ICollection<IRuleMatcher> Values => _ruleMatchersByName.Values;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, IRuleMatcher>> GetEnumerator()
    {
        return _ruleMatchersByName.GetEnumerator();
    }

    public void Add(KeyValuePair<string, IRuleMatcher> item)
    {
        this[item.Key] = item.Value;
    }

    public void Clear()
    {
        _ruleMatchersByName.Clear();
        _transientRulesKeys.Clear();
        _ruleResultTypesByName.Clear();
    }

    public bool Contains(KeyValuePair<string, IRuleMatcher> item)
    {
        return _ruleMatchersByName.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, IRuleMatcher>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, IRuleMatcher>>) _ruleMatchersByName).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, IRuleMatcher> item)
    {
        _transientRulesKeys.Remove(item.Key);

        return _ruleMatchersByName.Remove(item.Key) && _ruleResultTypesByName.Remove(item.Key);
    }

    public void Add(string key, IRuleMatcher value)
    {
        this[key] = value;
    }

    public bool ContainsKey(string key)
    {
        return _ruleMatchersByName.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        _transientRulesKeys.Remove(key);

        return _ruleMatchersByName.Remove(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IRuleMatcher value)
    {
        return _ruleMatchersByName.TryGetValue(key, out value);
    }
}