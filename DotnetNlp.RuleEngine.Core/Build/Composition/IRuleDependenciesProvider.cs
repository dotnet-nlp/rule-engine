using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation;

namespace DotnetNlp.RuleEngine.Core.Build.Composition;

public interface IRuleDependenciesProvider
{
    IReadOnlySet<string> GetDependencies(IRuleSpace forRuleSpace);
    IReadOnlySet<IChainedMemberAccessToken>? GetDependenciesOnRuleSpaceParameters();
}