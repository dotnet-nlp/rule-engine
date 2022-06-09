using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class RuleToken : IRuleToken
{
    public string? Namespace { get; }
    public ICSharpTypeToken ReturnType { get; }
    public string Name { get; }
    public CSharpParameterToken[] RuleParameters { get; }
    public string PatternKey { get; }
    public IPatternToken Pattern { get; }
    public IProjectionToken Projection { get; }

    public RuleToken(
        string? @namespace,
        ICSharpTypeToken returnType,
        string name,
        CSharpParameterToken[] ruleParameters,
        string patternKey,
        IPatternToken pattern,
        IProjectionToken projection
    )
    {
        Namespace = @namespace;
        ReturnType = returnType;
        Name = name;
        RuleParameters = ruleParameters;
        PatternKey = patternKey;
        Pattern = pattern;
        Projection = projection;
    }

    public override string ToString()
    {
        return $"{ReturnType} {Name} = {PatternKey}#{Pattern}# {Projection}";
    }
}