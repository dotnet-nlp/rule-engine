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
    RuleMatchResultCollection Match(RuleInput input, int firstSymbolIndex, IRuleSpaceCache cache);
    RuleMatchResultCollection MatchAndProject(
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache cache
    );
}

public static class RuleMatcherExtensions
{
    public static RuleMatchResultCollection MatchAll(
        this IRuleMatcher matcher,
        RuleInput input,
        int firstSymbolIndex,
        IRuleSpaceCache cache
    )
    {
        return EvaluateAll(
            startIndex => matcher.Match(input, startIndex, cache),
            input,
            firstSymbolIndex
        );
    }

    public static RuleMatchResultCollection MatchAndProjectAll(
        this IRuleMatcher matcher,
        RuleInput input,
        int firstSymbolIndex,
        RuleArguments ruleArguments,
        IRuleSpaceCache cache
    )
    {
        return EvaluateAll(
            startIndex => matcher.MatchAndProject(input, startIndex, ruleArguments, cache),
            input,
            firstSymbolIndex
        );
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