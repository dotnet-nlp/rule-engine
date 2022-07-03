using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Tests.Helpers;

public class DummyRuleSpace : IRuleSpace
{
    public static readonly DummyRuleSpace Instance = new();
    public static readonly IRuleSpaceDescription DescriptionInstance = new RuleSpaceBasedRuleSpaceDescription(Instance);

    private DummyRuleSpace()
    {
    }

    public IReadOnlyDictionary<string, Type> RuleSpaceParameterTypesByName => ImmutableDictionary<string, Type>.Empty;
    public IReadOnlyDictionary<string, Type> RuleResultTypesByName => ImmutableDictionary<string, Type>.Empty;
    public IReadOnlyDictionary<string, IRuleMatcher> RuleMatchersByName => ImmutableDictionary<string, IRuleMatcher>.Empty;

    public IRuleMatcher this[string ruleName]
    {
        get => throw new Exception();
        set => throw new Exception();
    }

    public int Count => throw new NotImplementedException();
    public bool IsReadOnly => throw new NotImplementedException();
    public ICollection<string> Keys => throw new NotImplementedException();
    public ICollection<IRuleMatcher> Values => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, IRuleMatcher>> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public void Add(KeyValuePair<string, IRuleMatcher> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<string, IRuleMatcher> item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<string, IRuleMatcher>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<string, IRuleMatcher> item)
    {
        throw new NotImplementedException();
    }

    public void Add(string key, IRuleMatcher value)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(string key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(string key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(string key, out IRuleMatcher value)
    {
        throw new NotImplementedException();
    }
}