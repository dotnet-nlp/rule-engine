using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

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
        string[] sequence,
        ref int index,
        in PegProcessorDataCollector dataCollector,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    )
    {
        var boundIndex = index;

        var result = _child.Match(sequence, ref boundIndex, dataCollector, ruleSpaceArguments, cache);

        return result != _lookaheadToken.IsNegative;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _child.GetUsedWords();
    }
}