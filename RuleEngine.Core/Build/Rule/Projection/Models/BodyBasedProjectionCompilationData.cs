using System;
using System.Collections.Generic;
using RuleEngine.Core.Evaluation.Rule.Projection.Parameters;

namespace RuleEngine.Core.Build.Rule.Projection.Models;

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