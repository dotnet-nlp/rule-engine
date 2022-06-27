using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Reflection;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.Transitions;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Visualization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Visualization.Formatters;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Graph.Walker;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;

internal sealed class RegexAutomaton : IUsedWordsProvider
{
    public RegexAutomatonState StartState { get; private set; }
    public RegexAutomatonState EndState { get; private set; }

    public RegexAutomaton(RegexAutomatonState startState, RegexAutomatonState endState)
    {
        StartState = startState;
        EndState = endState;
    }

    public IEnumerable<string> GetUsedWords()
    {
        var visitedStateIds = new HashSet<int>();

        return GetUsedWordsFromNewTransitions(StartState);

        IEnumerable<string> GetUsedWordsFromNewTransitions(RegexAutomatonState state)
        {
            var usedWords = Enumerable.Empty<string>();

            if (visitedStateIds.Add(state.Id))
            {
                foreach (var transition in state.OutgoingTransitions)
                {
                    usedWords = usedWords.Concat(transition.Payload.GetUsedWords());
                    usedWords = usedWords.Concat(GetUsedWordsFromNewTransitions(transition.TargetState));
                }
            }

            return usedWords;
        }
    }

    public Uri VisualizeAs(VisualizationFormat format)
    {
        (IGraphFormatter<string> Formatter, string FileExtension) formatterData = format switch
        {
            VisualizationFormat.Gv => (DotGraphFormatter.Instance, "gv"),
            VisualizationFormat.Svg => (SvgFormatter.Instance, "svg"),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported format")
        };

        return DigraphVisualizer.Instance.Format(
            "Regex automaton",
            RecursiveDfsDigraphWalker
                .Instance
                .DiscoverGraph<RegexAutomatonState, RegexAutomatonTransition>(StartState),
            RegexAutomatonLabelProvider.Instance,
            new SavingFormatter(formatterData.Formatter, formatterData.FileExtension)
        );
    }

    public static void AddTransition(RegexAutomatonTransition transition)
    {
        transition.SourceState.OutgoingTransitions.Add(transition);
        transition.TargetState.IncomingTransitions.Add(transition);
    }

    public void Reverse(IDigraphWalker digraphWalker)
    {
        var digraph = digraphWalker
            .DiscoverGraph<RegexAutomatonState, RegexAutomatonTransition>(StartState);

        foreach (var transition in digraph.Edges.Values)
        {
            transition.Reverse();
        }

        (StartState, EndState) = (EndState, StartState);
    }
}