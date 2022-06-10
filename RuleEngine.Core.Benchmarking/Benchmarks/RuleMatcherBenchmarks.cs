using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using RuleEngine.Core.Lib.Common;
using RuleEngine.Core.Lib.Common.Helpers;
using RuleEngine.Mechanics.Peg.Build.InputProcessing;
using RuleEngine.Mechanics.Peg.Build.Tokenization;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using RuleEngine.Mechanics.Regex.Build.InputProcessing;
using RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using RuleEngine.Mechanics.Regex.Build.Tokenization;
using RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace RuleEngine.Core.Benchmarking.Benchmarks;

public class RuleMatcherBenchmarks
{
    private readonly Dictionary<string, string[]> _phrases;

    private readonly IRuleSpace _ruleSpace;

    private readonly Consumer _consumer = new();

    public RuleMatcherBenchmarks()
    {
        _ruleSpace = CreateRuleSpace();
        _phrases = DataProvider.MatcherCases.PhrasesByRule;

        IRuleSpace CreateRuleSpace()
        {
            var stringInterner = new StringInterner();

            var factory = new RuleSpaceFactory(
                new []
                {
                    new MechanicsBundle(
                        "peg",
                        new LoopBasedPegPatternTokenizer(stringInterner),
                        new PegProcessorFactory(
                            new CombinedStrategy(
                                new IResultSelectionStrategy[]
                                {
                                    new MaxExplicitSymbolsStrategy(),
                                    new MaxProgressStrategy(),
                                }
                            )
                        ),
                        typeof(PegGroupToken)
                    ),
                    new MechanicsBundle(
                        "regex",
                        new LoopBasedRegexPatternTokenizer(stringInterner, new ErrorIndexHelper(Environment.NewLine)),
                        new RegexProcessorFactory(OptimizationLevel.Max),
                        typeof(RegexGroupToken)
                    ),
                }
            );

            return factory.CreateWithAliases(
                new []
                {
                    factory.RuleSetTokenizer.Tokenize(DataProvider.MatcherCases.RuleSets["ner.time"], "ner.time", true),
                    factory.RuleSetTokenizer.Tokenize(DataProvider.MatcherCases.RuleSets["ner.doctor"], "ner.doctor", true),
                },
                Array.Empty<RuleToken>(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                ImmutableDictionary<string, IRuleSpace>.Empty,
                ImmutableDictionary<string, Type>.Empty,
                new LoadedAssembliesProvider()
            );
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("AllCases")]
    public void RuleMatcher()
    {
        foreach (var (ruleName, phrase) in _phrases)
        {
            var ruleInput = new RuleInput(
                phrase,
                new RuleSpaceArguments(ImmutableDictionary<string, object?>.Empty)
            );

            _consumer.Consume(
                _ruleSpace[ruleName]
                    .MatchAndProject(
                        ruleInput,
                        0,
                        new RuleArguments(ImmutableDictionary<string, object?>.Empty),
                        new RuleSpaceCache()
                    )
            );
        }
    }
}