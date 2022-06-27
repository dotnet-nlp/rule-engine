using System;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DotnetNlp.RuleEngine.Core;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Benchmarking.Benchmarks;

public class RegexInputProcessorBenchmarks
{
    private readonly (IRuleMatcher Matcher, string[] Phrases)[] _automatonWalker_classic;
    private readonly (IRuleMatcher Matcher, string[] Phrases)[] _automatonWalker_real;

    private readonly Consumer _consumer = new();

    public RegexInputProcessorBenchmarks()
    {
        _automatonWalker_classic = DataProvider.Classic.Select(TransformCase).ToArray();
        _automatonWalker_real = DataProvider.Real.Select(TransformCase).ToArray();

        (IRuleMatcher Matcher, string[] Phrases) TransformCase((string Regex, string[] Phrases) @case)
        {
            return (CreateMatcher(@case.Regex), @case.Phrases);

            IRuleMatcher CreateMatcher(string regex)
            {
                var stringInterner = new StringInterner();

                var factory = new RuleSpaceFactory(
                    new []
                    {
                        new MechanicsBundle(
                            "regex",
                            new LoopBasedRegexPatternTokenizer(stringInterner, new ErrorIndexHelper(Environment.NewLine)),
                            new RegexProcessorFactory(OptimizationLevel.Max),
                            typeof(RegexGroupToken)
                        ),
                    }
                );

                const string runeName = "foo";

                var ruleSpace = factory.CreateWithAliases(
                    Array.Empty<RuleSetToken>(),
                    new []
                    {
                        new RuleToken(
                            null,
                            new ResolvedCSharpTypeToken("string", typeof(string)),
                            runeName,
                            Array.Empty<CSharpParameterToken>(),
                            "regex",
                            factory.PatternTokenizers["regex"].Tokenize(regex, null, false),
                            MatchedInputBasedProjectionToken.Instance
                        ),
                    },
                    ImmutableDictionary<string, IRuleMatcher>.Empty,
                    ImmutableDictionary<string, IRuleSpace>.Empty,
                    ImmutableDictionary<string, Type>.Empty,
                    new LoadedAssembliesProvider()
                );

                return ruleSpace[runeName];
            }
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("RegexInputProcessor_ClassicCases")]
    public void AutomatonWalker_Classic()
    {
        RunTest(_automatonWalker_classic);
    }

    [Benchmark(Baseline = true), BenchmarkCategory("RegexInputProcessor_RealCases")]
    public void AutomatonWalker_Real()
    {
        RunTest(_automatonWalker_real);
    }

    private void RunTest((IRuleMatcher Matcher, string[] Phrases)[] cases)
    {
        foreach (var @case in cases)
        {
            foreach (var phrase in @case.Phrases)
            {
                @case
                    .Matcher
                    .MatchAndProject(
                        new RuleInput(
                            phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                            new RuleSpaceArguments(ImmutableDictionary<string, object?>.Empty)
                        ),
                        0,
                        new RuleArguments(ImmutableDictionary<string, object?>.Empty),
                        new RuleSpaceCache()
                    )
                    .Consume(_consumer);
            }
        }
    }
}