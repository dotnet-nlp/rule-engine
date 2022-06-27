using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Formatters;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Label;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization;

internal sealed class DigraphVisualizer : IDigraphVisualizer
{
    public static readonly DigraphVisualizer Instance = new();

    private DigraphVisualizer()
    {
    }

    public TResult Format<TVertex, TEdge, TResult>(
        string name,
        Digraph<TVertex, TEdge> digraph,
        ILabelProvider<TVertex, TEdge> labelProvider,
        IGraphFormatter<TResult> formatter
    )
        where TVertex : IDigraphVertex<TVertex, TEdge>
        where TEdge : IDigraphEdge<TVertex, TEdge>
    {
        var graph = CreateModel(name.Replace(' ', '_'), digraph, labelProvider);

        return formatter.Format(graph);
    }

    private static GraphModel CreateModel<TVertex, TEdge>(
        string name,
        Digraph<TVertex, TEdge> digraph,
        ILabelProvider<TVertex, TEdge> labelProvider
    )
        where TVertex : IDigraphVertex<TVertex, TEdge>
        where TEdge : IDigraphEdge<TVertex, TEdge>
    {
        var nodeModels = digraph
            .Vertices
            .MapValue(vertex => new NodeModel(vertex.Id, labelProvider.GetLabel(vertex)))
            .ToDictionary();

        var edgeSourceNodesByEdgeId = digraph
            .Vertices
            .Values
            .SelectMany(
                vertex => vertex
                    .Edges
                    .Select(edge => new KeyValuePair<int, NodeModel>(edge.Id, nodeModels[vertex.Id]))
            )
            .Distinct(new EntityEqualityComparer<KeyValuePair<int,NodeModel>, int>(pair => pair.Key))
            .ToDictionary();

        IReadOnlyCollection<EdgeModel> edgeModels = digraph
            .Edges
            .Values
            .Select(
                edge => new EdgeModel(
                    labelProvider.GetLabel(edge),
                    edgeSourceNodesByEdgeId[edge.Id],
                    nodeModels[edge.TargetVertex.Id]
                )
            )
            .ToArray();

        return new GraphModel(name, nodeModels, edgeModels);
    }
}