using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Rule.Projection.Models;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Projection;

internal interface IProjectionCompiler
{
    IRuleProjection CreateProjection(
        string ruleKey,
        IProjectionCompilationData data,
        IAssembliesProvider assembliesProvider
    );

    Dictionary<string, IRuleProjection> CreateProjections(
        IDictionary<string, IProjectionCompilationData> dataByRuleName,
        IAssembliesProvider assembliesProvider,
        int extraCapacity = 0
    );
}