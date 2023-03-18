using System;
using System.Threading;
using BenchmarkDotNet.Running;
using DotnetNlp.RuleEngine.Mechanics.Peg.Benchmarking.Benchmarks;
using DotnetNlp.Tools.Benchmarking;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Benchmarking;

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
        // BenchmarkRunner.Run<NerBenchmarks>(Configs.Default, args);
        // BenchmarkRunner.Run<TerminalDetectorBenchmarks>(Configs.Default, args);
    }

    private static void Profile()
    {
        var c = new NerBenchmarks();
        GC.Collect(2, GCCollectionMode.Forced, true, true);

        for (var i = 0; i < 100000; i++)
        {
            Thread.Sleep(3000);
            c.Main();
            Thread.Sleep(3000);
        }
    }
}