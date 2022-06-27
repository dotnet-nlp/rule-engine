using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Formatters;

internal interface IGraphFormatter<out TResult>
{
    TResult Format(GraphModel graph);
}