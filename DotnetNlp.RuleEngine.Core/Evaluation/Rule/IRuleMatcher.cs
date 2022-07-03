using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
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
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    );

    RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    );

    public bool HasMatch(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return Match(sequence, firstSymbolIndex, ruleSpaceArguments, ruleArguments, cache).Count > 0;
    }

    public RuleMatchResultCollection MatchAll(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return EvaluateAll(
            startIndex => Match(sequence, startIndex, ruleSpaceArguments, ruleArguments, cache),
            sequence,
            firstSymbolIndex
        );
    }

    public RuleMatchResultCollection MatchAndProjectAll(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return EvaluateAll(
            startIndex => MatchAndProject(sequence, startIndex, ruleSpaceArguments, ruleArguments, cache),
            sequence,
            firstSymbolIndex
        );
    }

    public bool HasAnyMatch(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return MatchAll(sequence, firstSymbolIndex, ruleSpaceArguments, ruleArguments, cache).Count > 0;
    }

    private static RuleMatchResultCollection EvaluateAll(
        Func<int, RuleMatchResultCollection> evaluator,
        string[] sequence,
        int firstSymbolIndex
    )
    {
        if (firstSymbolIndex == sequence.Length)
        {
            return evaluator(firstSymbolIndex);
        }

        var results = new List<RuleMatchResultCollection>(sequence.Length);

        for (var symbolIndex = firstSymbolIndex; symbolIndex < sequence.Length; symbolIndex++)
        {
            results.Add(evaluator(symbolIndex));
        }

        return results.Merge();
    }
}