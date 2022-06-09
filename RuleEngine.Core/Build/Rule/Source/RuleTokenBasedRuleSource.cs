using System;
using RuleEngine.Core.Build.InputProcessing;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Evaluation.Rule.Projection;
using RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using RuleEngine.Core.Evaluation.Rule.Result;
using RuleEngine.Core.Exceptions;

namespace RuleEngine.Core.Build.Rule.Source;

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

    public IRuleMatcher GetRuleMatcher(in IRuleSpace ruleSpace)
    {
        try
        {
            return new RuleMatcher(
                _inputProcessorFactory.Create(_rule.Pattern, ruleSpace, _ruleSpaceDescription),
                _ruleParameters,
                _capturedVariablesParameters,
                _resultDescription,
                _ruleProjection
            );
        }
        catch (Exception exception)
        {
            throw new RuleBuildException(
                $"Cannot create rule matcher for rule '{_rule.GetFullName()}'.",
                exception
            );
        }
    }
}