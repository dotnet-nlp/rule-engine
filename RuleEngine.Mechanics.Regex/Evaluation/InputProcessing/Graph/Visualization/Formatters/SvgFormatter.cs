using System;
using System.IO;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Helpers;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Models;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Formatters;

internal sealed class SvgFormatter : IGraphFormatter<string>
{
    public static readonly SvgFormatter Instance = new();

    private const string FileExtension = "svg";

    public string Format(GraphModel graph)
    {
        var dotGraph = DotGraphFormatter.Instance.Format(graph);

        var pathToGvFile = new Uri(PathHelper.GetTempFilePath("gv"));

        File.WriteAllText(pathToGvFile.AbsolutePath, dotGraph);

        var pathToResultFile = new Uri(PathHelper.GetTempFilePath(FileExtension));

        DotHelper.RunDot(FileExtension, pathToGvFile, pathToResultFile);

        return File.ReadAllText(pathToResultFile.AbsolutePath);
    }
}