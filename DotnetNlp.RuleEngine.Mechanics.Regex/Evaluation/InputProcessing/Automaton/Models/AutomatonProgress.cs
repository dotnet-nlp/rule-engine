using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models.States;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Evaluation.InputProcessing.Automaton.Models;

/// <remarks>
/// Performance remarks: library performance depends on the way the fields in this class are declared.
/// Please make sure you know what you are doing, when changing any of the fields declaration.
/// </remarks>
internal sealed class AutomatonProgress
{
    public readonly int LastUsedSymbolIndex;
    public Dictionary<string, object?>? CapturedVariables;
    public readonly int ExplicitlyMatchedSymbolsCount;
    public readonly string? Marker;
    public readonly Func<object?>? CapturedValueFactory;
    public readonly RegexAutomatonState State;

    public AutomatonProgress(
        int lastUsedSymbolIndex,
        Dictionary<string, object?>? capturedVariables,
        int explicitlyMatchedSymbolsCount,
        string? marker,
        Func<object?>? capturedValueFactory,
        RegexAutomatonState state
    )
    {
        LastUsedSymbolIndex = lastUsedSymbolIndex;
        CapturedVariables = capturedVariables;
        ExplicitlyMatchedSymbolsCount = explicitlyMatchedSymbolsCount;
        Marker = marker;
        CapturedValueFactory = capturedValueFactory;
        State = state;
    }

    // as we can't now define nullable of nullable,
    // we're forced to use additional parameters to mark if nullable value should be replaces
    public AutomatonProgress Clone(
        RegexAutomatonState state,
        int? lastUsedSymbolIndex = null,
        int? explicitlyMatchedSymbolsCount = null,
        Dictionary<string, object?>? capturedVariables = null,
        bool replaceCapturedVariables = false,
        string? marker = null,
        bool replaceMarker = false,
        Func<object?>? capturedValueFactory = null,
        bool replaceCapturedValueFactory = false
    )
    {
        return new AutomatonProgress(
            lastUsedSymbolIndex ?? LastUsedSymbolIndex,
            replaceCapturedVariables ? capturedVariables : CapturedVariables?.ToDictionary(),
            explicitlyMatchedSymbolsCount ?? ExplicitlyMatchedSymbolsCount,
            replaceMarker ? marker : Marker,
            replaceCapturedValueFactory ? capturedValueFactory : CapturedValueFactory,
            state
        );
    }

    public RuleMatchResult Flush(RuleInput ruleInput, int firstSymbolIndex)
    {
        return new RuleMatchResult(
            ruleInput.Sequence,
            firstSymbolIndex,
            LastUsedSymbolIndex,
            CapturedVariables,
            ExplicitlyMatchedSymbolsCount,
            Marker,
            RuleMatchResult.LazyNull
        );
    }

    public void AddCapturedVariable(string variableName, object? variableValue)
    {
        CapturedVariables ??= new Dictionary<string, object?>();

        CapturedVariables[variableName] = variableValue;
    }
}