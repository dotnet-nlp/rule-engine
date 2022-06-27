namespace DotnetNlp.RuleEngine.Core.Build.Rule.Projection.Models;

internal sealed class VoidProjectionCompilationData : IProjectionCompilationData
{
    public static readonly VoidProjectionCompilationData Instance = new();

    private VoidProjectionCompilationData()
    {
    }
}