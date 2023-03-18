using System;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DotnetNlp.RuleEngine.Core;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Benchmarking.Data;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Benchmarking.Benchmarks;

public class NerBenchmarks
{
    private readonly IRuleMatcher _matcher;
    private readonly string[][] _cases;

    private readonly Consumer _consumer = new();

    public NerBenchmarks()
    {
        _matcher = CreateRuleMatcher();
        _cases = CreateCases();
    }

    [Benchmark]
    public void Main()
    {
        foreach (var @case in _cases)
        {
            _consumer.Consume(_matcher.MatchAndProject(@case).Single().Result.Value);
        }
    }

    private static IRuleMatcher CreateRuleMatcher()
    {
        var factory = new RuleSpaceFactory(
            new[]
            {
                new MechanicsDescription(
                    "peg",
                    new LoopBasedPegPatternTokenizer(new StringInterner(), new ErrorIndexHelper("\r\n")),
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
            }
        );

        const string nerName = "sdn.RelativeTimeSpanNer";

        var ruleSpace = factory.Create(
            "ner_benchmarks",
            new []{ factory.RuleSetTokenizer.Tokenize(RelativeTimeSpanNer.Declaration, nerName, false) },
            Array.Empty<IRuleToken>(),
            ImmutableDictionary<string, IRuleMatcher>.Empty,
            Array.Empty<IRuleSpace>(),
            ImmutableDictionary<string, Type>.Empty,
            LoadedAssembliesProvider.Instance
        );

        return ruleSpace[nerName];
    }

    private static string[][] CreateCases()
    {
        return new[]
        {
            "за полчаса",
            "через полтора часа",
            "через два часа",
            "за пять часов десять минут",
            "за час тридцать пять минут",
            "за час тридцать пять",
            "за пятнадцать минут",
            "через двадцать минут",
            "за два с половиной часа",
            "через пять с половиной часов",
            "за полчаса",
            "через пол часа",
            "за полтора часа",
            "через один с половиной час",
            "за два с половиной часа",
            "через пять с половиной часов",
            "за девять с половиной часов",
            "через двенадцать с половиной часов",
            "за час ноль две минуты",
            "через один час ноль девять минут",
            "за два часа пятнадцать минут",
            "через три часа двадцать минут",
            "за пять часов сорок одну минута",
            "через двенадцать часов пятьдесят девять минут",
            "за час ноль пять",
            "через два ноль две",
            "за пять пятнадцать",
            "через семь тридцать пять",
            "за час",
            "через два часа",
            "за пять часов",
            "через двенадцать часов",
            "через две минуты",
            "за пять минут",
            "через тридцать минут",
            "за пятьдесят пять минут",
        }
        .Select(phrase => phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        .ToArray();
    }
}