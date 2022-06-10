using System;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;
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

public class RuleSpaceFactoryBenchmarks
{
    private readonly RuleSpaceFactory _ruleSpaceFactory;

    private readonly (RuleSetToken[] RuleSets, RuleToken[] Rules)[] _cases;

    private readonly Consumer _consumer = new();

    public RuleSpaceFactoryBenchmarks()
    {
        _ruleSpaceFactory = CreateFactory();

        _cases = new []
        {
            (
                new []
                {
                    _ruleSpaceFactory.RuleSetTokenizer.Tokenize(DataProvider.MatcherCases.RuleSets["ner.time"], "ner.time", true),
                    _ruleSpaceFactory.RuleSetTokenizer.Tokenize(DataProvider.MatcherCases.RuleSets["ner.doctor"], "ner.doctor", true),
                },
                Array.Empty<RuleToken>()
            ),
        };

        RuleSpaceFactory CreateFactory()
        {
            var stringInterner = new StringInterner();
            var errorIndexHelper = new ErrorIndexHelper(Environment.NewLine);

            return new RuleSpaceFactory(
                new[]
                {
                    new MechanicsBundle(
                        "peg",
                        new LoopBasedPegPatternTokenizer(stringInterner, errorIndexHelper),
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
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("AllCases")]
    public void RuleSpaceFactory()
    {
        foreach (var (ruleSets, rules) in _cases)
        {
            _consumer.Consume(
                _ruleSpaceFactory.CreateWithAliases(
                    ruleSets,
                    rules,
                    ImmutableDictionary<string, IRuleMatcher>.Empty,
                    ImmutableDictionary<string, IRuleSpace>.Empty,
                    ImmutableDictionary<string, Type>.Empty,
                    new LoadedAssembliesProvider()
                )
            );
        }
    }
}