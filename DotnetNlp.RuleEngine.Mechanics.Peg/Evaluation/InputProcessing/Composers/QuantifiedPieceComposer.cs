using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
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
        string[] sequence,
        ref int index,
        in PegProcessorDataCollector dataCollector,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    )
    {
        var hasToCaptureVariable = _variableName is not null;
        var resultsCollector = hasToCaptureVariable
            ? new QuantifiableResultsCollector(_quantifiable.ResultType, _quantifier.Max)
            : null;

        if (Quantify(sequence, ref index, dataCollector, resultsCollector, ruleSpaceArguments, cache))
        {
            if (hasToCaptureVariable)
            {
                dataCollector.AddCapturedVariable(_variableName!, resultsCollector!.GetOverallResult());
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
        string[] sequence,
        ref int index,
        in PegProcessorDataCollector dataCollector,
        in QuantifiableResultsCollector? resultsCollector,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    )
    {
        for (var i = 0; i < _quantifier.Min; i++)
        {
            var isMatched = _quantifiable.TryParse(
                sequence,
                ref index,
                out var explicitlyMatchedSymbolsCount,
                out var result,
                ruleSpaceArguments,
                cache
            );

            if (isMatched)
            {
                dataCollector.ExplicitlyMatchedSymbolsCount += explicitlyMatchedSymbolsCount;
                resultsCollector?.AddLocalResult(result);
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
                    sequence,
                    ref index,
                    out var explicitlyMatchedSymbolsCount,
                    out var result,
                    ruleSpaceArguments,
                    cache
                );

                if (isMatched)
                {
                    dataCollector.ExplicitlyMatchedSymbolsCount += explicitlyMatchedSymbolsCount;
                    resultsCollector?.AddLocalResult(result);
                }
                else
                {
                    break;
                }
            }

            return true;
        }

        while (_quantifiable.TryParse(sequence, ref index, out var explicitlyMatchedSymbolsCount, out var result, ruleSpaceArguments: ruleSpaceArguments, cache: cache))
        {
            dataCollector.ExplicitlyMatchedSymbolsCount += explicitlyMatchedSymbolsCount;
            resultsCollector?.AddLocalResult(result);
        }

        return true;
    }

    private class QuantifiableResultsCollector
    {
        private readonly Type _unaryResultType;
        private readonly int? _maxResultsCount;
        private List<object?>? _localResults;

        public QuantifiableResultsCollector(Type unaryResultType, int? maxResultsCount)
        {
            _unaryResultType = unaryResultType;
            _maxResultsCount = maxResultsCount;
            _localResults = null;
        }

        public void AddLocalResult(object? result)
        {
            _localResults ??= _maxResultsCount is null
                ? new List<object?>()
                : new List<object?>(_maxResultsCount.Value);

            _localResults.Add(result);
        }

        public object? GetOverallResult()
        {
            if (_maxResultsCount == 1)
            {
                if (_localResults is null)
                {
                    return default;
                }

                if (_localResults.Count > 1)
                {
                    throw new PegProcessorMatchException(
                        $"Invalid matching results count {_localResults.Count} (expected 0 or 1)."
                    );
                }

                return _localResults.SingleOrDefault();
            }

            var (list, addMethod) = CSharpCodeHelper.CreateGenericList(_unaryResultType);

            if (_localResults is not null)
            {
                foreach (var unaryResult in _localResults.Where(localResult => localResult is not null))
                {
                    addMethod(unaryResult);
                }
            }

            return list;
        }
    }
}