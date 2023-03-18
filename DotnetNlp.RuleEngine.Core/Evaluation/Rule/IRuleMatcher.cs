using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
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

    IReadOnlySet<string> GetDependencies(IRuleSpace forRuleSpace);

    /// <summary>
    /// As this method may be highly loaded, the convention here is to return null (instead of empty set),
    /// if there's no dependencies.
    /// </summary>
    IReadOnlySet<IChainedMemberAccessToken>? GetDependenciesOnRuleSpaceParameters();

    RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    );

    public RuleMatchResultCollection FullMatch(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return Match(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache).GetFullMatches();
    }

    public RuleMatchResultCollection MatchAll(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return EvaluateAll(
            startIndex => Match(sequence, startIndex, ruleArguments, ruleSpaceArguments, cache),
            sequence,
            firstSymbolIndex
        );
    }

    RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    );

    public RuleMatchResultCollection FullMatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return MatchAndProject(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache).GetFullMatches();
    }

    public RuleMatchResultCollection MatchAndProjectAll(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return EvaluateAll(
            startIndex => MatchAndProject(sequence, startIndex, ruleArguments, ruleSpaceArguments, cache),
            sequence,
            firstSymbolIndex
        );
    }

    public bool HasMatch(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return Match(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache).Count > 0;
    }

    public bool HasFullMatch(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return Match(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache).GetFullMatches().Count > 0;
    }

    public bool HasAnyMatch(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return MatchAll(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache).Count > 0;
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