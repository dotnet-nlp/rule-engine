using BenchmarkDotNet.Running;
using RuleEngine.Core.Benchmarking.Benchmarks;
using Tools.Benchmarking;

namespace RuleEngine.Core.Benchmarking;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 1 && args[0] == "profile")
        {
            Profile();
        }
        else
        {
            Benchmark(args);
        }
    }

    private static void Benchmark(string[] args)
    {
        // this will run all the benchmarks
        BenchmarkRunner.Run(typeof(Program).Assembly, Configs.Default, args);

        // this will run single benchmark
        // BenchmarkRunner.Run<RuleSetTokenizerBenchmarks>(Configs.Default, args);
        // BenchmarkRunner.Run<RuleMatcherBenchmarks>(Configs.Default, args);
        // BenchmarkRunner.Run<RuleSpaceFactoryBenchmarks>(Configs.Default, args);
    }

    private static void Profile()
    {
        new RuleMatcherBenchmarks().RuleMatcher();
        // new RuleSetTokenizerBenchmarks().RuleSetTokenizer("ner.specialties");
    }
}