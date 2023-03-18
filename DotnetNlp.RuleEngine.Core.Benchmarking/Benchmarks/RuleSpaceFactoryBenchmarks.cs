using System;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Benchmarking.Benchmarks;

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
                    new MechanicsDescription(
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
                    new MechanicsDescription(
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
                _ruleSpaceFactory.Create(
                    Guid.NewGuid().ToString(),
                    ruleSets,
                    rules,
                    ImmutableDictionary<string, IRuleMatcher>.Empty,
                    Array.Empty<IRuleSpace>(),
                    ImmutableDictionary<string, Type>.Empty,
                    LoadedAssembliesProvider.Instance
                )
            );
        }
    }
}