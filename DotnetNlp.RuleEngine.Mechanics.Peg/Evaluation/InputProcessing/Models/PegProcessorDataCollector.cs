using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

/// <remarks>
/// Performance remarks: library performance depends on the way the fields in this class are declared.
/// Please make sure you know what you are doing, when changing any of the fields declaration.
/// </remarks>
public sealed class PegProcessorDataCollector
{
    public int ExplicitlyMatchedSymbolsCount;
    public Dictionary<string, object?>? CapturedVariables;

    public PegProcessorDataCollector()
    {
        ExplicitlyMatchedSymbolsCount = 0;
        CapturedVariables = null;
    }

    public void AddCapturedVariable(string variableName, object? variableValue)
    {
        CapturedVariables ??= new Dictionary<string, object?>();

        CapturedVariables[variableName] = variableValue;
    }
}