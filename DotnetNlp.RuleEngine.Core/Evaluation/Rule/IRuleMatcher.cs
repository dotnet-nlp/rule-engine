using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule;

public interface IRuleMatcher : IUsedWordsProvider
{
    RuleParameters Parameters { get; }
    RuleMatchResultDescription ResultDescription { get; }
    RuleMatchResultCollection Match(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    );

    RuleMatchResultCollection MatchAndProject(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    );

    public bool HasMatch(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    )
    {
        return Match(input, firstSymbolIndex, ruleArguments, cache).Count > 0;
    }

    public RuleMatchResultCollection MatchAll(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    )
    {
        return EvaluateAll(
            startIndex => Match(input, startIndex, ruleArguments, cache),
            input,
            firstSymbolIndex
        );
    }

    public RuleMatchResultCollection MatchAndProjectAll(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    )
    {
        return EvaluateAll(
            startIndex => MatchAndProject(input, startIndex, ruleArguments, cache),
            input,
            firstSymbolIndex
        );
    }

    public bool HasAnyMatch(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache? cache = null
    )
    {
        return MatchAll(input, firstSymbolIndex, ruleArguments, cache).Count > 0;
    }

    private static RuleMatchResultCollection EvaluateAll(
        Func<int, RuleMatchResultCollection> evaluator,
        RuleInput input,
        int firstSymbolIndex
    )
    {
        if (firstSymbolIndex == input.Sequence.Length)
        {
            return evaluator(firstSymbolIndex);
        }

        var results = new List<RuleMatchResultCollection>(input.Sequence.Length);

        for (var symbolIndex = firstSymbolIndex; symbolIndex < input.Sequence.Length; symbolIndex++)
        {
            results.Add(evaluator(symbolIndex));
        }

        return results.Merge();
    }
}