﻿namespace DotnetNlp.RuleEngine.Core.Build.Rule.Projection.Models;

internal sealed class ConstantProjectionCompilationData : IProjectionCompilationData
{
    public object? Constant { get; }

    public ConstantProjectionCompilationData(object? constant)
    {
        Constant = constant;
    }
}