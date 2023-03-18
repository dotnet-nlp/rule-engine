using System;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Exceptions;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Source;

internal sealed class RuleTokenBasedRuleSource : IRuleSource
{
    private readonly RuleToken _rule;
    private readonly IInputProcessorFactory _inputProcessorFactory;
    private readonly RuleParameters _ruleParameters;
    private readonly CapturedVariablesParameters _capturedVariablesParameters;
    private readonly RuleMatchResultDescription _resultDescription;
    private readonly IRuleProjection _ruleProjection;
    private readonly IRuleSpaceDescription _ruleSpaceDescription;

    public RuleTokenBasedRuleSource(
        RuleToken rule,
        IInputProcessorFactory inputProcessorFactory,
        RuleParameters ruleParameters,
        CapturedVariablesParameters capturedVariablesParameters,
        RuleMatchResultDescription resultDescription,
        IRuleProjection ruleProjection,
        IRuleSpaceDescription ruleSpaceDescription
    )
    {
        _rule = rule;
        _inputProcessorFactory = inputProcessorFactory;
        _ruleParameters = ruleParameters;
        _capturedVariablesParameters = capturedVariablesParameters;
        _resultDescription = resultDescription;
        _ruleProjection = ruleProjection;
        _ruleSpaceDescription = ruleSpaceDescription;
    }

    public IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace, Action<Action> subscribeOnRuleSpaceCreated)
    {
        try
        {
            return new RuleMatcher(
                _inputProcessorFactory.Create(
                    _rule.Pattern,
                    ruleSpace,
                    _ruleSpaceDescription,
                    subscribeOnRuleSpaceCreated
                ),
                _ruleParameters,
                _capturedVariablesParameters,
                _resultDescription,
                _ruleProjection
            );
        }
        catch (Exception exception)
        {
            throw new RuleBuildException(
                $"Cannot create rule matcher for rule '{((IRuleToken) _rule).GetFullName()}'.",
                exception
            );
        }
    }
}