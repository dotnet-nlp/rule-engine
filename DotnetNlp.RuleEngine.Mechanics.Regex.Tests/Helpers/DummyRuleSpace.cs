using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotnetNlp.RuleEngine.Core.Build;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Tests.Helpers;

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
}