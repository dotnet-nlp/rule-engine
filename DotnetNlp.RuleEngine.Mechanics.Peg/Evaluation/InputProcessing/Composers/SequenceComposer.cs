using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal sealed class SequenceComposer : IComposer
{
    private readonly IReadOnlyCollection<IComposer> _pieces;

    public SequenceComposer(IReadOnlyCollection<IComposer> pieces)
    {
        _pieces = pieces;
    }

    public bool Match(
        string[] sequence,
        ref int index,
        in PegProcessorDataCollector dataCollector,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    )
    {
        foreach (var piece in _pieces)
        {
            if (!piece.Match(sequence, ref index, dataCollector, ruleSpaceArguments, cache))
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