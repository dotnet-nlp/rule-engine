using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Evaluation;
using RuleEngine.Core.Evaluation.Rule;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;

namespace RuleEngine.Core.Tests.Helpers;

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