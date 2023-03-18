using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Bundle;

public static class Factory
{
    public static IRuleSpace Create(
        string? name = null,
        IReadOnlyDictionary<string, string>? ruleSets = null,
        IReadOnlyDictionary<string, string>? rules = null,
        IReadOnlyDictionary<string, IRuleMatcher>? matchers = null,
        IReadOnlyCollection<IRuleSpace>? ruleSpaces = null,
        IReadOnlyDictionary<string, Type>? ruleSpaceParameterTypes = null,
        IAssembliesProvider? assembliesProvider = null,
        IReadOnlyCollection<MechanicsDescription>? mechanics = null
    )
    {
        var ruleSpaceFactory = new RuleSpaceFactory(
            mechanics ?? new[]
            {
                Mechanics.Peg,
                Mechanics.Regex,
            }
        );

        return ruleSpaceFactory.Create(
            name ?? Guid.NewGuid().ToString(),
            ruleSets
                ?.MapValue((key, ruleSet) => ruleSpaceFactory.RuleSetTokenizer.Tokenize(ruleSet, key, false))
                .SelectValues()
                .ToArray() ?? Array.Empty<RuleSetToken>(),
            rules
                ?.MapValue(rule => ruleSpaceFactory.PatternTokenizers[Mechanics.Regex.Key].Tokenize(rule, null, false))
                .MapValue(
                    (key, pattern) => new RuleToken(
                        null,
                        VoidProjectionToken.ReturnType,
                        key,
                        Array.Empty<CSharpParameterToken>(),
                        Mechanics.Regex.Key,
                        pattern,
                        VoidProjectionToken.Instance
                    )
                )
                .SelectValues()
                .ToArray() ?? Array.Empty<IRuleToken>(),
            matchers ?? (IReadOnlyDictionary<string, IRuleMatcher>) ImmutableDictionary<string, IRuleMatcher>.Empty,
            ruleSpaces ?? Array.Empty<IRuleSpace>(),
            ruleSpaceParameterTypes ?? (IReadOnlyDictionary<string, Type>) ImmutableDictionary<string, Type>.Empty,
            assembliesProvider ?? LoadedAssembliesProvider.Instance
        );
    }
}