using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.InputProcessing;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing;

internal sealed class PegProcessor : IInputProcessor
{
    private readonly OrderedChoiceComposer _root;
    private readonly IReadOnlySet<string> _dependencies;

    public PegProcessor(OrderedChoiceComposer root, IReadOnlySet<string> dependencies)
    {
        _root = root;
        _dependencies = dependencies;
    }

    public IReadOnlySet<string> GetDependencies()
    {
        return _dependencies;
    }

    public RuleMatchResultCollection Match(
        string[] sequence,
        int firstSymbolIndex = 0,
        RuleSpaceArguments? ruleSpaceArguments = null,
        IRuleSpaceCache? cache = null
    )
    {
        var dataCollector = new PegInputProcessorDataCollector();

        var nextSymbolIndex = firstSymbolIndex;

        var isMatched = _root.Match(sequence, ref nextSymbolIndex, dataCollector, ruleSpaceArguments, cache);

        if (isMatched)
        {
            return new RuleMatchResultCollection(
                new []
                {
                    new RuleMatchResult(
                        sequence,
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