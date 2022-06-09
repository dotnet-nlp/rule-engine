using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal sealed class OrderedChoiceComposer : IComposer
{
    private readonly IReadOnlyCollection<IComposer> _choices;

    public OrderedChoiceComposer(IReadOnlyCollection<IComposer> choices)
    {
        _choices = choices;
    }

    public bool Match(
        RuleInput input,
        ref int index,
        in PegInputProcessorDataCollector dataCollector,
        IRuleSpaceCache cache
    )
    {
        foreach (var choice in _choices)
        {
            var choiceBoundIndex = index;

            if (choice.Match(input, ref choiceBoundIndex, dataCollector, cache))
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