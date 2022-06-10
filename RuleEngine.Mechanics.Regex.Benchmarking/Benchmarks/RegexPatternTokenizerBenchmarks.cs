using System;
using System.Linq;
using RuleEngine.Mechanics.Regex.Build.Tokenization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using RuleEngine.Core.Lib.Common;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Mechanics.Regex.Benchmarking.Benchmarks;

public class RegexPatternTokenizerBenchmarks
{
    private readonly string[] _cases_classic;
    private readonly string[] _cases_real;

    private readonly LoopBasedRegexPatternTokenizer _regexPatternTokenizer;

    private readonly Consumer _consumer = new();

    public RegexPatternTokenizerBenchmarks()
    {
        _cases_classic = DataProvider.Classic.Select(TransformCase).ToArray();
        _cases_real = DataProvider.Real.Select(TransformCase).ToArray();

        _regexPatternTokenizer = new LoopBasedRegexPatternTokenizer(new StringInterner(), new ErrorIndexHelper(Environment.NewLine));

        string TransformCase((string Regex, string[] Phrases) @case)
        {
            return @case.Regex;
        }
    }

    [Benchmark, BenchmarkCategory("RegexPatternTokenizer_ClassicCases")]
    public void Tokenizer_Classic()
    {
        foreach (var regex in _cases_classic)
        {
            var token = _regexPatternTokenizer.Tokenize(regex, null, false);

            _consumer.Consume(token);
        }
    }

    [Benchmark, BenchmarkCategory("RegexPatternTokenizer_RealCases")]
    public void Tokenizer_Real()
    {
        foreach (var regex in _cases_real)
        {
            var token = _regexPatternTokenizer.Tokenize(regex, null, false);

            _consumer.Consume(token);
        }
    }
}