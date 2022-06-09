using System;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using RuleEngine.Core.Evaluation.Rule.Result;
using RuleEngine.Core.Exceptions;

namespace RuleEngine.Core.Build.Rule.Source;

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

    public IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace)
    {
        try
        {
            return new EmptyRuleMatcher(_ruleParameters, _resultDescription);
        }
        catch (Exception exception)
        {
            throw new RuleBuildException(
                $"Cannot create empty rule matcher for rule '{_rule.GetFullName()}'.",
                exception
            );
        }
    }
}