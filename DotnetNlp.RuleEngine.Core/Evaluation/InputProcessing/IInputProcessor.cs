using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;

public interface IInputProcessor : IUsedWordsProvider
{
    IReadOnlySet<string> GetDependencies();

    RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    );
}