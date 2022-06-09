using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.TerminalDetectors;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace RuleEngine.Mechanics.Peg.Benchmarking.Benchmarks;

public class TerminalDetectorBenchmarks
{
    private const int RunsNumber = 10000000;

    private readonly AnyLiteralDetector _anyLiteralDetector;
    private readonly LiteralDetector _literalDetector;
    private readonly LiteralSetDetector _literalSetDetector;
    private readonly LiteralSetDetector _negatedLiteralSetDetector;

    private readonly Consumer _consumer = new();

    public TerminalDetectorBenchmarks()
    {
        _anyLiteralDetector = AnyLiteralDetector.Instance;
        _literalDetector = new LiteralDetector(new LiteralToken("привет"));
        _literalSetDetector = new LiteralSetDetector(new LiteralSetToken(false, new ILiteralSetMemberToken[]{new LiteralToken("привет")}));
        _negatedLiteralSetDetector = new LiteralSetDetector(new LiteralSetToken(true, new ILiteralSetMemberToken[]{new LiteralToken("привет")}));
    }

    [Benchmark, BenchmarkCategory("TerminalDetectorBenchmarks_Positive")]
    public void AnyLiteralDetector_Positive()
    {
        for (var i = 0; i < RunsNumber; i++)
        {
            var isMatched = _anyLiteralDetector.WordMatches("привет");

            _consumer.Consume(isMatched);
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("TerminalDetectorBenchmarks_Positive")]
    public void LiteralDetector_Positive()
    {
        for (var i = 0; i < RunsNumber; i++)
        {
            var isMatched = _literalDetector.WordMatches("привет");

            _consumer.Consume(isMatched);
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("TerminalDetectorBenchmarks_Negative")]
    public void LiteralDetector_Negative()
    {
        for (var i = 0; i < RunsNumber; i++)
        {
            var isMatched = _literalDetector.WordMatches("пока");

            _consumer.Consume(isMatched);
        }
    }

    [Benchmark, BenchmarkCategory("TerminalDetectorBenchmarks_Positive")]
    public void LiteralSetDetector_Positive()
    {
        for (var i = 0; i < RunsNumber; i++)
        {
            var isMatched = _literalSetDetector.WordMatches("привет");

            _consumer.Consume(isMatched);
        }
    }

    [Benchmark, BenchmarkCategory("TerminalDetectorBenchmarks_Negative")]
    public void LiteralSetDetector_Negative()
    {
        for (var i = 0; i < RunsNumber; i++)
        {
            var isMatched = _literalSetDetector.WordMatches("пока");

            _consumer.Consume(isMatched);
        }
    }

    [Benchmark, BenchmarkCategory("TerminalDetectorBenchmarks_Positive")]
    public void NegatedLiteralSetDetector_Positive()
    {
        for (var i = 0; i < RunsNumber; i++)
        {
            var isMatched = _negatedLiteralSetDetector.WordMatches("пока");

            _consumer.Consume(isMatched);
        }
    }

    [Benchmark, BenchmarkCategory("TerminalDetectorBenchmarks_Negative")]
    public void NegatedLiteralSetDetector_Negative()
    {
        for (var i = 0; i < RunsNumber; i++)
        {
            var isMatched = _negatedLiteralSetDetector.WordMatches("привет");

            _consumer.Consume(isMatched);
        }
    }
}