using System;
using RuleEngine.Core.Evaluation.Cache;
using RuleEngine.Core.Evaluation.Rule.Input;
using RuleEngine.Core.Reflection;

namespace RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

internal interface IQuantifiableParser : IUsedWordsProvider
{
    Type ResultType { get; }
    bool TryParse(
        RuleInput input,
        IRuleSpaceCache cache,
        ref int index,
        out int explicitlyMatchedSymbolsCount,
        out object? result
    );
}