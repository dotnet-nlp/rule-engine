using System;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Source;

internal sealed class EmptyRuleTokenBasedRuleSource : IRuleSource
{
    private readonly EmptyRuleToken _rule;
    private readonly RuleParameters _ruleParameters;
    private readonly RuleMatchResultDescription _resultDescription;

    public EmptyRuleTokenBasedRuleSource(
        EmptyRuleToken rule,
        RuleParameters ruleParameters,
        RuleMatchResultDescription resultDescription
    )
    {
        _rule = rule;
        _ruleParameters = ruleParameters;
        _resultDescription = resultDescription;
    }

    public IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace, Action<Action> subscribeOnRuleSpaceCreated)
    {
        try
        {
            return new EmptyRuleMatcher(_ruleParameters, _resultDescription);
        }
        catch (Exception exception)
        {
            throw new RuleBuildException(
                $"Cannot create empty rule matcher for rule '{((IRuleToken) _rule).GetFullName()}'.",
                exception
            );
        }
    }
}