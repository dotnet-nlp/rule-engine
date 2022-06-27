using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Models;

public sealed class PegInputProcessorDataCollector
{
    public int ExplicitlyMatchedSymbolsCount { get; set; }
    public Dictionary<string, object?> CapturedVariables { get; }

    public PegInputProcessorDataCollector()
    {
        ExplicitlyMatchedSymbolsCount = 0;
        CapturedVariables = new Dictionary<string, object?>();
    }
}