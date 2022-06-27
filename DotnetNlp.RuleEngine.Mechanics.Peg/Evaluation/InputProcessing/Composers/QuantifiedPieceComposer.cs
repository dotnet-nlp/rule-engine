using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;
using DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;
using DotnetNlp.RuleEngine.Mechanics.Peg.Exceptions;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Composers;

internal sealed class QuantifiedPieceComposer : IComposer
{
    private readonly IQuantifiableParser _quantifiable;
    private readonly QuantifierToken _quantifier;
    private readonly string? _variableName;

    public QuantifiedPieceComposer(
        IQuantifiableParser quantifiable,
        QuantifierToken quantifier,
        string? variableName
    )
    {
        _quantifiable = quantifiable;
        _quantifier = quantifier;
        _variableName = variableName;
    }

    public bool Match(
        RuleInput input,
        ref int index,
        in PegInputProcessorDataCollector dataCollector,
        IRuleSpaceCache cache
    )
    {
        var resultsCollector = _variableName is null
            ? null
            : new QuantifiableResultsCollector(_quantifiable.ResultType, _quantifier.Max);

        if (Quantify(input, ref index, dataCollector, resultsCollector, cache))
        {
            if (resultsCollector is not null)
            {
                dataCollector
                    .CapturedVariables
                    .Add(_variableName!, resultsCollector.GetResult());
            }

            return true;
        }

        return false;
    }

    public IEnumerable<string> GetUsedWords()
    {
        return _quantifiable.GetUsedWords();
    }

    private bool Quantify(
        RuleInput input,
        ref int index,
        in PegInputProcessorDataCollector dataCollector,
        in QuantifiableResultsCollector? resultsCollector,
        IRuleSpaceCache cache
    )
    {
        for (var i = 0; i < _quantifier.Min; i++)
        {
            var isMatched = _quantifiable.TryParse(
                input,
                cache,
                ref index,
                out var explicitlyMatchedSymbolsCount,
                out var result
            );

            if (isMatched)
            {
                dataCollector.ExplicitlyMatchedSymbolsCount += explicitlyMatchedSymbolsCount;
                resultsCollector?.LocalResults.Add(result);
            }
            else
            {
                return false;
            }
        }

        if (_quantifier.Max is not null)
        {
            for (var i = _quantifier.Min; i < _quantifier.Max.Value; i++)
            {
                var isMatched = _quantifiable.TryParse(
                    input,
                    cache,
                    ref index,
                    out var explicitlyMatchedSymbolsCount,
                    out var result
                );

                if (isMatched)
                {
                    dataCollector.ExplicitlyMatchedSymbolsCount += explicitlyMatchedSymbolsCount;
                    resultsCollector?.LocalResults.Add(result);
                }
                else
                {
                    break;
                }
            }

            return true;
        }

        while (_quantifiable.TryParse(input, cache, ref index, out var explicitlyMatchedSymbolsCount, out var result))
        {
            dataCollector.ExplicitlyMatchedSymbolsCount += explicitlyMatchedSymbolsCount;
            resultsCollector?.LocalResults.Add(result);
        }

        return true;
    }

    private class QuantifiableResultsCollector
    {
        private readonly Type _unaryResultType;
        private readonly bool _resultTypeIsCollection;
        public List<object?> LocalResults { get; }

        public QuantifiableResultsCollector(Type unaryResultType, int? resultsCount)
        {
            _unaryResultType = unaryResultType;
            _resultTypeIsCollection = resultsCount != 1;
            LocalResults = resultsCount is null ? new List<object?>() : new List<object?>(resultsCount.Value);
        }

        public object? GetResult()
        {
            if (!_resultTypeIsCollection)
            {
                if (LocalResults.Count > 1)
                {
                    throw new PegProcessorMatchException(
                        $"Invalid matching results count {LocalResults.Count} (expected 0 or 1)."
                    );
                }

                return LocalResults.SingleOrDefault();
            }

            var (list, addMethod) = CSharpCodeHelper.CreateGenericList(_unaryResultType);

            foreach (var unaryResult in LocalResults.Where(localResult => localResult is not null))
            {
                addMethod(unaryResult);
            }

            return list;
        }
    }
}