using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule;

public sealed class StaticRuleMatcher<TResult> : IRuleMatcher
{
    private readonly Func<IEnumerable<string>> _usedWordsProvider;
    private readonly Func<object?[], IEnumerable<(TResult Result, int LastUsedSymbolIndex)>> _ruleEvaluator;

    public RuleParameters Parameters { get; }
    public RuleMatchResultDescription ResultDescription { get; }

    public StaticRuleMatcher(
        Func<IEnumerable<string>> usedWordsProvider,
        Func<object?[], IEnumerable<(TResult Result, int LastUsedSymbolIndex)>> ruleEvaluator,
        RuleParameters parameters
    )
    {
        _usedWordsProvider = usedWordsProvider;
        _ruleEvaluator = ruleEvaluator;
        Parameters = parameters;
        ResultDescription = new RuleMatchResultDescription(typeof(TResult), ImmutableDictionary<string, Type>.Empty);
    }

    public IReadOnlySet<string> GetDependencies(IRuleSpace forRuleSpace)
    {
        return ImmutableHashSet<string>.Empty;
    }

    public IReadOnlySet<IChainedMemberAccessToken>? GetDependenciesOnRuleSpaceParameters()
    {
        return null;
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return MatchAndProject(sequence, firstSymbolIndex, ruleArguments, ruleSpaceArguments, cache);
    }

    public RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleArguments? ruleArguments = null,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return new RuleMatchResultCollection(
            Evaluate(sequence, firstSymbolIndex, ruleArguments)
                .Select(
                    evaluationResult => new RuleMatchResult(
                        sequence,
                        firstSymbolIndex,
                        evaluationResult.LastUsedSymbolIndex,
                        null,
                        evaluationResult.LastUsedSymbolIndex - firstSymbolIndex + 1,
                        null,
                        new Lazy<object?>(() => evaluationResult.Result)
                    )
                )
        );
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _usedWordsProvider();
    }

    private IEnumerable<(TResult Result, int LastUsedSymbolIndex)> Evaluate(
        string[] sequence,
        int firstSymbolIndex,
        RuleArguments? ruleArguments
    )
    {
        if (firstSymbolIndex >= sequence.Length)
        {
            return Enumerable.Empty<(TResult Result, int LastUsedSymbolIndex)>();
        }

        // todo [realtime performance] we can use more efficient way of creating this variable
        var arguments = new object?[] { sequence, firstSymbolIndex };
        if (ruleArguments is not null)
        {
            arguments = arguments
                .Concat(ruleArguments.Values.SelectValues())
                .ToArray();
        }

        return _ruleEvaluator(arguments);
    }
}