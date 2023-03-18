using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build.Composition;

public sealed class RuleDependenciesProvider : IRuleDependenciesProvider
{
    private readonly IReadOnlySet<string> _dependencies;
    private readonly IReadOnlySet<IChainedMemberAccessToken> _dependenciesOnRuleSpaceParameters;
    private readonly int _parentRuleSpaceId;
    private readonly string _parentRuleSpaceName;

    public RuleDependenciesProvider(
        IReadOnlySet<string> dependencies,
        IReadOnlySet<IChainedMemberAccessToken> dependenciesOnRuleSpaceParameters,
        int parentRuleSpaceId,
        string parentRuleSpaceName
    )
    {
        _dependencies = dependencies;
        _dependenciesOnRuleSpaceParameters = dependenciesOnRuleSpaceParameters;
        _parentRuleSpaceId = parentRuleSpaceId;
        _parentRuleSpaceName = parentRuleSpaceName;
    }

    public IReadOnlySet<string> GetDependencies(IRuleSpace forRuleSpace)
    {
        if (_parentRuleSpaceId == forRuleSpace.Id)
        {
            return _dependencies;
        }

        return IterateDependencies(forRuleSpace.TransientRulesKeys).ToHashSet();
    }

    public IReadOnlySet<IChainedMemberAccessToken>? GetDependenciesOnRuleSpaceParameters()
    {
        return _dependenciesOnRuleSpaceParameters.NullIfEmpty<IReadOnlySet<IChainedMemberAccessToken>, IChainedMemberAccessToken>();
    }

    private IEnumerable<string> IterateDependencies(IReadOnlySet<string> ignoreKeys)
    {
        foreach (var dependency in _dependencies)
        {
            if (ignoreKeys.Contains(dependency))
            {
                yield return dependency;
            }
            else
            {
                yield return $"{_parentRuleSpaceName}.{dependency}";
            }
        }
    }
}