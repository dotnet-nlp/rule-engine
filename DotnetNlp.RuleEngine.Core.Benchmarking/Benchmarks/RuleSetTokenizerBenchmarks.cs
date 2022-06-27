using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using DotnetNlp.RuleEngine.Core.Build.Tokenization;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization;

namespace DotnetNlp.RuleEngine.Core.Benchmarking.Benchmarks;

public class RuleSetTokenizerBenchmarks
{
    private readonly LoopBasedRuleSetTokenizer _ruleSetTokenizer;

    public RuleSetTokenizerBenchmarks()
    {
        var stringInterner = new StringInterner();
        var errorIndexHelper = new ErrorIndexHelper(Environment.NewLine);
        _ruleSetTokenizer = new LoopBasedRuleSetTokenizer(
            new Dictionary<string, IPatternTokenizer>
            {
                {"peg", new LoopBasedPegPatternTokenizer(stringInterner, errorIndexHelper)},
                {"regex", new LoopBasedRegexPatternTokenizer(stringInterner, new ErrorIndexHelper(Environment.NewLine))},
            },
            errorIndexHelper
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