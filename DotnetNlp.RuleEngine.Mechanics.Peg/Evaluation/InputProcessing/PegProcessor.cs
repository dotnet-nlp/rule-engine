using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing;

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