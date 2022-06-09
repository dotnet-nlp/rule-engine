using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Rule.Result;

namespace RuleEngine.Core.Evaluation.Cache;

public interface IRuleSpaceCache
{
    RuleMatchResultCollection? GetResult(
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        IReadOnlyDictionary<string, object?>? ruleArguments
    );

    void SetResult(
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        IReadOnlyDictionary<string, object?>? ruleArguments,
        RuleMatchResultCollection result
    );
}