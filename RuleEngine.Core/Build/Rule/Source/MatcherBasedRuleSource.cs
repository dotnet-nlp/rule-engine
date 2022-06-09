﻿using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;

namespace RuleEngine.Core.Build.Rule.Source;

internal sealed class MatcherBasedRuleSource : IRuleSource
{
    private readonly IRuleMatcher _matcher;

    public MatcherBasedRuleSource(IRuleMatcher matcher)
    {
        _matcher = matcher;
    }

    public IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace)
    {
        return _matcher;
    }
}