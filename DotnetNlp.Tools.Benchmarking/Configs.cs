using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace DotnetNlp.Tools.Benchmarking;

public static class Configs
{
    public static readonly IConfig Default = ManualConfig
        .Create(DefaultConfig.Instance)
        .AddJob(Job.Default.WithRuntime(CoreRuntime.Core60).AsBaseline())
        .AddDiagnoser(MemoryDiagnoser.Default)
        .AddColumn(CategoriesColumn.Default)
        .AddColumn(StatisticColumn.Mean)
        .AddColumn(StatisticColumn.Min)
        .AddColumn(StatisticColumn.P95)
        .AddColumn(StatisticColumn.Max)
        .AddColumn(StatisticColumn.StdDev)
        .AddColumn(StatisticColumn.Iterations)
        .WithOption(ConfigOptions.JoinSummary, true)
        .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared))
        .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory)
        .AddExporter(CsvMeasurementsExporter.Default)
        .AddExporter(RPlotExporter.Default);
}