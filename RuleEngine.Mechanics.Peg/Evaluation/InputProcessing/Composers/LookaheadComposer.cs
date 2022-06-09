using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal sealed class LookaheadComposer : IComposer
{
    private readonly LookaheadToken _lookaheadToken;
    private readonly IComposer _child;

    public LookaheadComposer(LookaheadToken lookaheadToken, IComposer child)
    {
        _lookaheadToken = lookaheadToken;
        _child = child;
    }

    public bool Match(
        RuleInput input,
        ref int index,
        in PegInputProcessorDataCollector dataCollector,
        IRuleSpaceCache cache
    )
    {
        var boundIndex = index;

        var result = _child.Match(input, ref boundIndex, dataCollector, cache);

        return result != _lookaheadToken.IsNegative;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _child.GetUsedWords();
    }
}