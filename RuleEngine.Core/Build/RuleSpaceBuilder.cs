using System;
using System.Collections.Generic;
using RuleEngine.Core.Build.Rule.Source;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Evaluation.Rule.Cache;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Build;

internal sealed class RuleSpaceBuilder
{
    private readonly IRuleSpaceDescription _ruleSpaceDescription;
    private readonly IReadOnlyDictionary<string, Type> _ruleSpaceParameterTypesByName;
    private readonly IReadOnlyDictionary<string, IRuleSource> _ruleSourcesByName;
    private readonly IReadOnlyDictionary<string, string> _ruleAliases;
    private readonly RuleSpaceFactory _ruleSpaceFactory;

    private RuleSpace? _ruleSpace;

    public RuleSpaceBuilder(
        IRuleSpaceDescription ruleSpaceDescription,
        IReadOnlyDictionary<string, Type> ruleSpaceParameterTypesByName,
        IReadOnlyDictionary<string, IRuleSource> ruleSourcesByName,
        IReadOnlyDictionary<string, string> ruleAliases,
        RuleSpaceFactory ruleSpaceFactory
    )
    {
        _ruleSpaceDescription = ruleSpaceDescription;
        _ruleSpaceParameterTypesByName = ruleSpaceParameterTypesByName;
        _ruleSourcesByName = ruleSourcesByName;
        _ruleAliases = ruleAliases;
        _ruleSpaceFactory = ruleSpaceFactory;
    }

    public IRuleSpace Build()
    {
        var ruleMatchers = new Dictionary<string, IRuleMatcher>(
            _ruleSourcesByName.Count + _ruleAliases.Count
        );

        _ruleSpace = new RuleSpace(
            _ruleSpaceParameterTypesByName,
            _ruleSpaceDescription.ResultTypesByRuleName.ToDictionary(),
            ruleMatchers
        );

        FillRuleMatchers(ruleMatchers);

        return _ruleSpace;
    }

    private void FillRuleMatchers(in Dictionary<string, IRuleMatcher> ruleMatchers)
    {
        foreach (var (ruleKey, ruleSource) in _ruleSourcesByName)
        {
            var ruleMatcher = ruleSource.GetRuleMatcher(_ruleSpace!);

            CachingRuleMatcher cachingRuleMatcher;

            if (ruleMatcher is CachingRuleMatcher caching)
            {
                cachingRuleMatcher = caching;
            }
            else
            {
                cachingRuleMatcher = _ruleSpaceFactory.WrapWithCache(ruleMatcher);
            }

            ruleMatchers.Add(ruleKey, cachingRuleMatcher);
        }

        ruleMatchers.AddAliases(_ruleAliases);
    }
}