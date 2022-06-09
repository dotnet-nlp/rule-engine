using System;
using System.IO;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Helpers;
using RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Models;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Formatters;

internal sealed class SavingFormatter : IGraphFormatter<Uri>
{
    private readonly IGraphFormatter<string> _underlyingFormatter;
    private readonly string _fileExtension;

    public SavingFormatter(
        IGraphFormatter<string> underlyingFormatter,
        string fileExtension
    )
    {
        _underlyingFormatter = underlyingFormatter;
        _fileExtension = fileExtension;
    }

    public Uri Format(GraphModel graph)
    {
        var formattedGraph = _underlyingFormatter.Format(graph);

        var pathToFile = PathHelper.GetTempFilePath(_fileExtension);

        File.WriteAllText(pathToFile, formattedGraph);

        return new Uri(pathToFile);
    }
}