using System.Collections.Generic;
using RuleEngine.Core.Build.Rule.Projection.Models;
using RuleEngine.Core.Evaluation.Rule.Projection;
using RuleEngine.Core.Lib.CodeAnalysis.Assemblies;

namespace RuleEngine.Core.Build.Rule.Projection;

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