using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

internal sealed class GroupParser : IQuantifiableParser
{
    public Type ResultType { get; } = typeof(void);

    private readonly OrderedChoiceComposer _composer;

    public GroupParser(OrderedChoiceComposer composer)
    {
        _composer = composer;
    }

    public bool TryParse(
        string[] sequence,
        ref int index,
        out int explicitlyMatchedSymbolsCount,
        out object? result,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    )
    {
        result = null;

        var dataCollector = new PegInputProcessorDataCollector();

        var isMatched = _composer.Match(sequence, ref index, dataCollector, ruleSpaceArguments, cache);

        explicitlyMatchedSymbolsCount = dataCollector.ExplicitlyMatchedSymbolsCount;

        return isMatched;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _composer.GetUsedWords();
    }
}