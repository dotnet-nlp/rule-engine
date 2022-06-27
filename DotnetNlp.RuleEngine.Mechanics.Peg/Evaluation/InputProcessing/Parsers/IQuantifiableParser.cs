using System;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

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