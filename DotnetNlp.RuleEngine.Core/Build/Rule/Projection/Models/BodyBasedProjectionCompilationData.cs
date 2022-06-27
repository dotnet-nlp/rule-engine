using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

namespace DotnetNlp.RuleEngine.Core.Build.Rule.Projection.Models;

internal sealed class BodyBasedProjectionCompilationData : IProjectionCompilationData
{
    public IReadOnlySet<string> Usings { get; }
    public Type ResultType { get; }
    public ProjectionParameters ProjectionParameters { get; }
    public string Body { get; }

    public BodyBasedProjectionCompilationData(
        IReadOnlySet<string> usings,
        Type resultType,
        ProjectionParameters projectionParameters,
        string body
    )
    {
        Usings = usings;
        ResultType = resultType;
        ProjectionParameters = projectionParameters;
        Body = body;
    }
}