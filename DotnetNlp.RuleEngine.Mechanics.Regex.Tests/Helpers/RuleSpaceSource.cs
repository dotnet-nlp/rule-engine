using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Tests.Helpers;

internal sealed class RuleSpaceSource<TPatternToken>
    where TPatternToken : IPatternToken
{
    private readonly RuleSpaceFactory _factory;
    private readonly IReadOnlyDictionary<string, (string Definition, TPatternToken Token)> _rules;
    private readonly IReadOnlyCollection<Type> _staticRuleContainers;

    private IRuleSpace? _ruleSpace;
    public IRuleSpace RuleSpace => _ruleSpace ??= CreateRuleSpace();

    public RuleSpaceSource(
        RuleSpaceFactory factory,
        IReadOnlyDictionary<string, (string Definition, TPatternToken Token)> rules,
        IReadOnlyCollection<Type> staticRuleContainers
    )
    {
        _factory = factory;
        _rules = rules;
        _staticRuleContainers = staticRuleContainers;
    }

    private IRuleSpace CreateRuleSpace()
    {
        return _factory.CreateWithAliases(
            Array.Empty<RuleSetToken>(),
            _rules
                .MapValue(
                    (ruleName, rulePattern) => new RuleToken(
                        null,
                        new ClassicCSharpTypeToken(new CSharpTypeNameWithNamespaceToken("void"), Array.Empty<ICSharpTypeToken>()),
                        ruleName,
                        Array.Empty<CSharpParameterToken>(),
                        "regex",
                        rulePattern.Token,
                        VoidProjectionToken.Instance
                    )
                )
                .SelectValues()
                .ToArray(),
            _staticRuleContainers
                .Select(_factory.StaticRuleFactory.ConvertStaticRuleContainerToRuleMatchers)
                .MergeWithKnownCapacity(_staticRuleContainers.Count),
            ImmutableDictionary<string, IRuleSpace>.Empty,
            ImmutableDictionary<string, Type>.Empty,
            new LoadedAssembliesProvider()
        );
    }
}