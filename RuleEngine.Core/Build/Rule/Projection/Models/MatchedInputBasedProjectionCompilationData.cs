namespace RuleEngine.Core.Build.Rule.Projection.Models;

internal sealed class MatchedInputBasedProjectionCompilationData : IProjectionCompilationData
{
    public static readonly MatchedInputBasedProjectionCompilationData Instance = new();

    private MatchedInputBasedProjectionCompilationData()
    {
    }
}