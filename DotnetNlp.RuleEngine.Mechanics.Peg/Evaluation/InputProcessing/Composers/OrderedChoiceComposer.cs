using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal sealed class OrderedChoiceComposer : IComposer
{
    private readonly IReadOnlyCollection<IComposer> _choices;

    public OrderedChoiceComposer(IReadOnlyCollection<IComposer> choices)
    {
        _choices = choices;
    }

    public bool Match(
        string[] sequence,
        ref int index,
        in PegProcessorDataCollector dataCollector,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    )
    {
        foreach (var choice in _choices)
        {
            var choiceBoundIndex = index;

            if (choice.Match(sequence, ref choiceBoundIndex, dataCollector, ruleSpaceArguments, cache))
            {
                index = choiceBoundIndex;
                return true;
            }
        }

        return false;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _choices.SelectMany(choice => choice.GetUsedWords());
    }
}