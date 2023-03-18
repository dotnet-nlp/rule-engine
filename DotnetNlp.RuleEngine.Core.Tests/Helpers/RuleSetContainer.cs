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
    public IReadOnlyCollection<MechanicsDescription> MechanicsCollection { get; }

    private IRuleSpace? _ruleSpace;
    public IRuleSpace RuleSpace
    {
        get
        {
            return _ruleSpace ??= new RuleSpaceFactory(MechanicsCollection).Create(
                Guid.NewGuid().ToString(),
                new[] { Token },
                Array.Empty<RuleToken>(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                Array.Empty<IRuleSpace>(),
                ImmutableDictionary<string, Type>.Empty,
                LoadedAssembliesProvider.Instance
            );
        }
    }

    public RuleSetContainer(
        string definition,
        RuleSetToken token,
        IReadOnlyCollection<MechanicsDescription> mechanicsCollection
    )
    {
        Definition = definition;
        Token = token;
        MechanicsCollection = mechanicsCollection;
    }
}