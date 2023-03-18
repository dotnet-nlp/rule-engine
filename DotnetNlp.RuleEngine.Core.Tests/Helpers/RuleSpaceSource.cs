using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Tests.Helpers;

internal sealed class RuleSpaceSource
{
    private readonly RuleSpaceFactory _factory;
    private readonly IReadOnlyDictionary<string, string> _rules;
    private readonly IReadOnlyDictionary<string, string> _ruleSets;

    private IRuleSpace? _ruleSpace;
    public IRuleSpace RuleSpace => _ruleSpace ??= CreateRuleSpace();

    public RuleSpaceSource(
        RuleSpaceFactory factory,
        IReadOnlyDictionary<string, string> rules,
        IReadOnlyDictionary<string, string> ruleSets
    )
    {
        _factory = factory;
        _rules = rules;
        _ruleSets = ruleSets;
    }

    private IRuleSpace CreateRuleSpace()
    {
        return _factory.Create(
            Guid.NewGuid().ToString(),
            _ruleSets
                .MapValue((ruleSetName, ruleSet) => _factory.RuleSetTokenizer.Tokenize(ruleSet, ruleSetName, false))
                .SelectValues()
                .ToArray(),
            _rules
                .MapValue(
                    (ruleName, rulePattern) => new RuleToken(
                        null,
                        VoidProjectionToken.ReturnType,
                        ruleName,
                        Array.Empty<CSharpParameterToken>(),
                        "regex",
                        _factory.PatternTokenizers["regex"].Tokenize(rulePattern, null, false),
                        VoidProjectionToken.Instance
                    )
                )
                .SelectValues()
                .ToArray(),
            ImmutableDictionary<string, IRuleMatcher>.Empty,
            Array.Empty<IRuleSpace>(),
            ImmutableDictionary<string, Type>.Empty,
            LoadedAssembliesProvider.Instance
        );
    }
}