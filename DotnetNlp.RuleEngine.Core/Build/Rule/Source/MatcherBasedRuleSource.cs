using System;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Source;

internal sealed class MatcherBasedRuleSource : IRuleSource
{
    private readonly IRuleMatcher _matcher;

    public MatcherBasedRuleSource(IRuleMatcher matcher)
    {
        _matcher = matcher;
    }

    public IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace, Action<Action> subscribeOnRuleSpaceCreated)
    {
        return _matcher;
    }
}