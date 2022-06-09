using System;
using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Exceptions;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Evaluation;

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
}