using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Result;
using RuleEngine.Core.Reflection;

namespace RuleEngine.Core.Evaluation.InputProcessing;

public interface IInputProcessor : IUsedWordsProvider
{
    RuleMatchResultCollection Match(RuleInput ruleInput, int firstSymbolIndex, IRuleSpaceCache cache);
}