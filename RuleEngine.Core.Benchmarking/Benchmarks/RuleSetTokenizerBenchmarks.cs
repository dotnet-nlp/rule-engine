using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using RuleEngine.Core.Build.Tokenization;
using RuleEngine.Core.Lib.Common;
using RuleEngine.Mechanics.Peg.Build.Tokenization;
using RuleEngine.Mechanics.Regex.Build.Tokenization;

namespace RuleEngine.Core.Benchmarking.Benchmarks;

public class RuleSetTokenizerBenchmarks
{
    private readonly LoopBasedRuleSetTokenizer _ruleSetTokenizer;

    public RuleSetTokenizerBenchmarks()
    {
        var stringInterner = new StringInterner();

        _ruleSetTokenizer = new LoopBasedRuleSetTokenizer(
            new Dictionary<string, IPatternTokenizer>
            {
                {"peg", new LoopBasedPegPatternTokenizer(stringInterner)},
                {"regex", new LoopBasedRegexPatternTokenizer(stringInterner)},
            }
        );
    }

    [Benchmark(Baseline = true), BenchmarkCategory("AllCases")]
    [ArgumentsSource(nameof(GetRuleSets))]
    public void RuleSetTokenizer(string ruleSetName)
    {
        _ruleSetTokenizer.Tokenize(DataProvider.MatcherCases.RuleSets[ruleSetName], null, true);
    }

    public static string[] GetRuleSets()
    {
        return new []
        {
            "ner.time",
            "ner.doctor",
        };
    }
}