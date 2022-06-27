using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;

namespace DotnetNlp.RuleEngine.Core.Tests.Helpers;

internal sealed class RuleSetContainer
{
    public string Definition { get; }
    public RuleSetToken Token { get; }
    public IReadOnlyCollection<MechanicsBundle> MechanicsCollection { get; }

    private IRuleSpace? _ruleSpace;
    public IRuleSpace RuleSpace
    {
        get
        {
            return _ruleSpace ??= new RuleSpaceFactory(MechanicsCollection).CreateWithAliases(
                new[] { Token },
                Array.Empty<RuleToken>(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                ImmutableDictionary<string, IRuleSpace>.Empty,
                ImmutableDictionary<string, Type>.Empty,
                new LoadedAssembliesProvider()
            );
        }
    }

    public RuleSetContainer(
        string definition,
        RuleSetToken token,
        IReadOnlyCollection<MechanicsBundle> mechanicsCollection
    )
    {
        Definition = definition;
        Token = token;
        MechanicsCollection = mechanicsCollection;
    }
}