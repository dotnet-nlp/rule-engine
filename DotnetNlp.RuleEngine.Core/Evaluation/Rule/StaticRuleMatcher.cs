using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;

namespace DotnetNlp.RuleEngine.Core.Evaluation.Rule;

public sealed class StaticRuleMatcher<TResult> : IRuleMatcher
{
    private readonly Func<IEnumerable<string>> _usedWordsProvider;
    private readonly Func<object?[], IEnumerable<(TResult Result, int LastUsedSymbolIndex)>> _ruleEvaluator;

    public IReadOnlySet<string> Dependencies => ImmutableHashSet<string>.Empty;
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
        ResultDescription = new RuleMatchResultDescription(
            typeof(TResult),
            ImmutableDictionary<string, Type>.Empty
        );
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        return MatchAndProject(sequence, firstSymbolIndex, ruleSpaceArguments, ruleArguments, cache);
    }

    public RuleMatchResultCollection MatchAndProject(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        RuleArguments? ruleArguments = null,
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
                .Concat(ruleArguments.Values.Values)
                .ToArray();
        }

        return _ruleEvaluator(arguments);
    }
}