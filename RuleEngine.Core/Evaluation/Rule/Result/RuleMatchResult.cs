using System;
using System.Collections.Generic;

namespace RuleEngine.Core.Evaluation.Rule.Result;

public sealed class RuleMatchResult
{
    public static readonly Lazy<object?> LazyNull = new(() => null);

    public IReadOnlyCollection<string> Source { get; }
    public int FirstUsedSymbolIndex { get; }
    public int LastUsedSymbolIndex { get; }
    public IReadOnlyDictionary<string, object?>? CapturedVariables { get; }
    public int ExplicitlyMatchedSymbolsCount { get; }
    public string? Marker { get; }
    public Lazy<object?> Result { get; }

    public RuleMatchResult(
        IReadOnlyCollection<string> source,
        int firstUsedSymbolIndex,
        int lastUsedSymbolIndex,
        IReadOnlyDictionary<string, object?>? capturedVariables,
        int explicitlyMatchedSymbolsCount,
        string? marker,
        Lazy<object?> result
    )
    {
        Source = source;
        FirstUsedSymbolIndex = firstUsedSymbolIndex;
        LastUsedSymbolIndex = lastUsedSymbolIndex;
        CapturedVariables = capturedVariables;
        ExplicitlyMatchedSymbolsCount = explicitlyMatchedSymbolsCount;
        Marker = marker;
        Result = result;
    }

    public bool TryGetCapturedVariable(string variableName, out object? variable)
    {
        if (CapturedVariables is not null && CapturedVariables.TryGetValue(variableName, out var capturedVariable))
        {
            variable = capturedVariable;
            return true;
        }

        variable = null;
        return false;
    }
}