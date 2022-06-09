namespace RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Models;

internal sealed class NodeModel
{
    public int Id { get; }
    public string Label { get; }

    public NodeModel(int id, string label)
    {
        Id = id;
        Label = label;
    }
}