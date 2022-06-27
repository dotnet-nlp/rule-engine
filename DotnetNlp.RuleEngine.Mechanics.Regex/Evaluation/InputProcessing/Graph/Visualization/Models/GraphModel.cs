using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Models;

internal sealed class GraphModel
{
    public string Name { get; }
    public IReadOnlyDictionary<int, NodeModel> Nodes { get; }
    public IReadOnlyCollection<EdgeModel> Edges { get; }

    public GraphModel(string name, IReadOnlyDictionary<int, NodeModel> nodes, IReadOnlyCollection<EdgeModel> edges)
    {
        Name = name;
        Nodes = nodes;
        Edges = edges;
    }
}