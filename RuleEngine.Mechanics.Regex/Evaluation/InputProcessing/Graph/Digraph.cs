using System.Collections.Generic;

namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph;

internal sealed class Digraph<TVertex, TEdge>
    where TVertex : IDigraphVertex<TVertex, TEdge>
    where TEdge : IDigraphEdge<TVertex, TEdge>
{
    public IReadOnlyDictionary<int, TVertex> Vertices { get; }
    public IReadOnlyDictionary<int, TEdge> Edges { get; }

    public Digraph(
        IReadOnlyDictionary<int, TVertex> vertices,
        IReadOnlyDictionary<int, TEdge> edges
    )
    {
        Vertices = vertices;
        Edges = edges;
    }
}