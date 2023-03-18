using System;
using System.Collections.Generic;
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
                    new MechanicsDescription(
                        "peg",
                        new LoopBasedPegPatternTokenizer(stringInterner, new ErrorIndexHelper(Environment.NewLine)),
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

            return factory.Create(
                Guid.NewGuid().ToString(),
                new []
                {
                    factory.RuleSetTokenizer.Tokenize(DataProvider.MatcherCases.RuleSets["ner.time"], "ner.time", true),
                    factory.RuleSetTokenizer.Tokenize(DataProvider.MatcherCases.RuleSets["ner.doctor"], "ner.doctor", true),
                },
                Array.Empty<RuleToken>(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                Array.Empty<IRuleSpace>(),
                ImmutableDictionary<string, Type>.Empty,
                LoadedAssembliesProvider.Instance
            );
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("AllCases")]
    public void RuleMatcher()
    {
        foreach (var (ruleName, phrase) in _phrases)
        {
            _consumer.Consume(_ruleSpace[ruleName].MatchAndProject(phrase));
        }
    }
}