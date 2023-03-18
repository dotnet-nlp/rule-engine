using System;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Reflection;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Evaluation.InputProcessing.Parsers;

internal interface IQuantifiableParser : IUsedWordsProvider
{
    Type ResultType { get; }

    bool TryParse(
        string[] sequence,
        ref int index,
        out int explicitlyMatchedSymbolsCount,
        out object? result,
        RuleSpaceArguments? ruleSpaceArguments,
        IRuleSpaceCache? cache
    );
}