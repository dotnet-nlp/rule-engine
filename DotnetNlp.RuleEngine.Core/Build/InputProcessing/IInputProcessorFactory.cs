using System;
using DotnetNlp.RuleEngine.Core.Build.InputProcessing.Models;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;

namespace DotnetNlp.RuleEngine.Core.Build.InputProcessing;

public interface IInputProcessorFactory
{
    IInputProcessor Create(
        IPatternToken patternToken,
        IRuleSpace ruleSpace,
        IRuleSpaceDescription ruleSpaceDescription,
        Action<Action> subscribeOnRuleSpaceCreated
    );

    RuleCapturedVariables ExtractOwnCapturedVariables(
        IPatternToken patternToken,
        IRuleSpaceDescription ruleSpaceDescription
    );
}