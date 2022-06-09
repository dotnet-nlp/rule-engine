using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.InputProcessing;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Evaluation.Rule.Result;
using RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;
using RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing;

internal sealed class PegProcessor : IInputProcessor
{
    private readonly OrderedChoiceComposer _root;

    public PegProcessor(OrderedChoiceComposer root)
    {
        _root = root;
    }

    public RuleMatchResultCollection Match(RuleInput ruleInput, int firstSymbolIndex, IRuleSpaceCache cache)
    {
        var dataCollector = new PegInputProcessorDataCollector();

        var nextSymbolIndex = firstSymbolIndex;

        var isMatched = _root.Match(ruleInput, ref nextSymbolIndex, dataCollector, cache);

        if (isMatched)
        {
            return new RuleMatchResultCollection(
                new []
                {
                    new RuleMatchResult(
                        ruleInput.Sequence,
                        firstSymbolIndex,
                        nextSymbolIndex - 1,
                        dataCollector.CapturedVariables,
                        dataCollector.ExplicitlyMatchedSymbolsCount,
                        null,
                        RuleMatchResult.LazyNull
                    ),
                }
            );
        }

        return new RuleMatchResultCollection(0);
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _root.GetUsedWords();
    }
}