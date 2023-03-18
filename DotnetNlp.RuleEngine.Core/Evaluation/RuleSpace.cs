using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation;

internal sealed class RuleSpace : IRuleSpace
{
    private readonly Dictionary<string, IRuleMatcher> _ruleMatchersByName;
    public IReadOnlyDictionary<string, IRuleMatcher> RuleMatchersByName => _ruleMatchersByName;
    private readonly Dictionary<string, Type> _ruleResultTypesByName;
    public IReadOnlyDictionary<string, Type> RuleResultTypesByName => _ruleResultTypesByName;

    public IReadOnlyDictionary<string, Type> RuleSpaceParameterTypesByName { get; }

    public IRuleMatcher this[string ruleName]
    {
        get => ResolveRule(ruleName);
        set => AddRule(ruleName, value);
    }

    public RuleSpace(
        IReadOnlyDictionary<string, Type> ruleSpaceParameterTypesByName,
        Dictionary<string, Type> ruleResultTypesByName,
        Dictionary<string, IRuleMatcher> ruleMatchersByName
    )
    {
        RuleSpaceParameterTypesByName = ruleSpaceParameterTypesByName;
        _ruleResultTypesByName = ruleResultTypesByName;
        _ruleMatchersByName = ruleMatchersByName;
    }

    public IReadOnlySet<string> Prune(IReadOnlyCollection<string> neededRules)
    {
        var visitedRules = new HashSet<string>();
        var rulesToPreserve = new List<string>();

        WaltThroughDependencyGraph(neededRules);

        var removedRules = new HashSet<string>();

        foreach (var pair in this.ExceptBy(rulesToPreserve, pair => pair.Key))
        {
            Remove(pair);
            removedRules.Add(pair.Key);
        }

        return removedRules;

        void WaltThroughDependencyGraph(IReadOnlyCollection<string> rules)
        {
            foreach (var rule in rules)
            {
                if (!visitedRules.Contains(rule))
                {
                    visitedRules.Add(rule);
                    rulesToPreserve.Add(rule);
                    WaltThroughDependencyGraph(this[rule].Dependencies);
                }
            }
        }

    }

    private void AddRule(string ruleName, IRuleMatcher value)
    {
        _ruleMatchersByName.Add(ruleName, value);
        _ruleResultTypesByName.Add(ruleName, value.ResultDescription.ResultType);
    }

    private IRuleMatcher ResolveRule(string ruleName)
    {
        if (RuleMatchersByName.TryGetValue(ruleName, out var ruleMatcher))
        {
            return ruleMatcher;
        }

        throw new RuleMatchException(
            $"Rule space doesn't contain rule {ruleName}. " +
            $"Available rules are: {RuleMatchersByName.Keys.JoinToString(", ")}."
        );
    }

    public int Count => _ruleMatchersByName.Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<string, IRuleMatcher>>) _ruleMatchersByName).IsReadOnly;
    public ICollection<string> Keys => ((IDictionary<string, IRuleMatcher>) _ruleMatchersByName).Keys;
    public ICollection<IRuleMatcher> Values => ((IDictionary<string, IRuleMatcher>) _ruleMatchersByName).Values;

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
        AddRule(item.Key, item.Value);
    }

    public void Clear()
    {
        _ruleMatchersByName.Clear();
        _ruleResultTypesByName.Clear();
    }

    public bool Contains(KeyValuePair<string, IRuleMatcher> item)
    {
        return _ruleMatchersByName.Contains(item);
    }

    void ICollection<KeyValuePair<string, IRuleMatcher>>.CopyTo(KeyValuePair<string, IRuleMatcher>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, IRuleMatcher>>) _ruleMatchersByName).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, IRuleMatcher> item)
    {
        return ((ICollection<KeyValuePair<string, IRuleMatcher>>) _ruleMatchersByName).Remove(item) &&
               _ruleMatchersByName.Remove(item.Key);
    }

    public void Add(string key, IRuleMatcher value)
    {
        AddRule(key, value);
    }

    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, IRuleMatcher>) _ruleMatchersByName).ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return ((IDictionary<string, IRuleMatcher>) _ruleMatchersByName).Remove(key);
    }

    public bool TryGetValue(string key, out IRuleMatcher value)
    {
        return ((IDictionary<string, IRuleMatcher>) _ruleMatchersByName).TryGetValue(key, out value!);
    }
}