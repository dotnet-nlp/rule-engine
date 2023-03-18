using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Rule.Source;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Cache;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build;

internal sealed class RuleSpaceBuilder
{
    private readonly string _ruleSpaceName;
    private readonly IRuleSpaceDescription _ruleSpaceDescription;
    private readonly IReadOnlyDictionary<string, Type> _ruleSpaceParameterTypesByName;
    private readonly IReadOnlyDictionary<string, IRuleSource> _ruleSourcesByName;
    private readonly IReadOnlyDictionary<string, string> _ruleAliases;
    private readonly RuleSpaceFactory _ruleSpaceFactory;
    private readonly HashSet<string> _transientRuleMatchersKeys;
    private readonly List<Action> _subscriptionsOnRuleSpaceCreated;

    private RuleSpace? _ruleSpace;

    public RuleSpaceBuilder(
        string ruleSpaceName,
        IRuleSpaceDescription ruleSpaceDescription,
        IReadOnlyDictionary<string, Type> ruleSpaceParameterTypesByName,
        IReadOnlyDictionary<string, IRuleSource> ruleSourcesByName,
        IReadOnlyDictionary<string, string> ruleAliases,
        RuleSpaceFactory ruleSpaceFactory,
        HashSet<string> transientRuleMatchersKeys
    )
    {
        _ruleSpaceName = ruleSpaceName;
        _ruleSpaceDescription = ruleSpaceDescription;
        _ruleSpaceParameterTypesByName = ruleSpaceParameterTypesByName;
        _ruleSourcesByName = ruleSourcesByName;
        _ruleAliases = ruleAliases;
        _ruleSpaceFactory = ruleSpaceFactory;
        _transientRuleMatchersKeys = transientRuleMatchersKeys;
        _subscriptionsOnRuleSpaceCreated = new List<Action>();
    }

    public IRuleSpace Build(int id)
    {
        var ruleMatchers = new Dictionary<string, IRuleMatcher>(
            _ruleSourcesByName.Count + _ruleAliases.Count
        );

        _ruleSpace = new RuleSpace(
            id,
            _ruleSpaceName,
            _ruleSpaceDescription.ResultTypesByRuleName.ToDictionary(),
            ruleMatchers,
            _transientRuleMatchersKeys,
            _ruleSpaceParameterTypesByName
        );

        FillRuleMatchers(ruleMatchers);

        foreach (var subscription in _subscriptionsOnRuleSpaceCreated)
        {
            subscription();
        }

        return _ruleSpace;
    }

    private void FillRuleMatchers(in Dictionary<string, IRuleMatcher> ruleMatchers)
    {
        foreach (var (ruleKey, ruleSource) in _ruleSourcesByName)
        {
            var ruleMatcher = ruleSource.GetRuleMatcher(_ruleSpace!, _subscriptionsOnRuleSpaceCreated.Add);

            var cachingRuleMatcher = ruleMatcher as CachingRuleMatcher ?? _ruleSpaceFactory.WrapWithCache(ruleMatcher);

            ruleMatchers.Add(ruleKey, cachingRuleMatcher);
        }

        ruleMatchers.AddAliases(_ruleAliases);
    }
}