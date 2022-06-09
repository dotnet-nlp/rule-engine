using RuleEngine.Core.Build.InputProcessing.Models;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.InputProcessing;

namespace RuleEngine.Core.Build.InputProcessing;

public interface IInputProcessorFactory
{
    IInputProcessor Create(IPatternToken patternToken, IRuleSpace ruleSpace, IRuleSpaceDescription ruleSpaceDescription);

    RuleCapturedVariables ExtractOwnCapturedVariables(
        IPatternToken patternToken,
        IRuleSpaceDescription ruleSpaceDescription
    );
}