using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph;

public interface IDigraphVertex<TVertex, TEdge>
    where TVertex : IDigraphVertex<TVertex, TEdge>
    where TEdge : IDigraphEdge<TVertex, TEdge>
{
    int Id { get; }
    IReadOnlyCollection<IDigraphEdge<TVertex, TEdge>> Edges { get; }
}