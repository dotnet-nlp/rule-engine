using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;

public interface IInputProcessor : IUsedWordsProvider
{
    RuleMatchResultCollection Match(RuleInput ruleInput, int firstSymbolIndex, IRuleSpaceCache cache);
}