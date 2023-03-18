using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Cache;

public interface IRuleSpaceCache
{
    RuleMatchResultCollection? GetResult(
        bool isProjected,
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        KeyValuePair<string, object?>[]? ruleArguments,
        IReadOnlyDictionary<string, object?>? ruleDependenciesOnRuleSpaceArguments
    );

    void SetResult(
        bool isProjected,
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        KeyValuePair<string, object?>[]? ruleArguments,
        IReadOnlyDictionary<string, object?>? ruleDependenciesOnRuleSpaceArguments,
        RuleMatchResultCollection result
    );
}