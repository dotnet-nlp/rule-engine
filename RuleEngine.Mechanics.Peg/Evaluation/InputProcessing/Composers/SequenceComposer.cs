using System.Collections.Generic;
using System.Linq;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal sealed class SequenceComposer : IComposer
{
    private readonly IReadOnlyCollection<IComposer> _pieces;

    public SequenceComposer(IReadOnlyCollection<IComposer> pieces)
    {
        _pieces = pieces;
    }

    public bool Match(
        RuleInput input,
        ref int index,
        in PegInputProcessorDataCollector dataCollector,
        IRuleSpaceCache cache
    )
    {
        foreach (var piece in _pieces)
        {
            if (!piece.Match(input, ref index, dataCollector, cache))
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _pieces.SelectMany(choice => choice.GetUsedWords());
    }
}