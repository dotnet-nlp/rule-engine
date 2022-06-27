using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;

public sealed class EmptyRuleToken : IRuleToken
{
    public string? Namespace { get; }
    public ICSharpTypeToken ReturnType { get; }
    public string Name { get; }
    public CSharpParameterToken[] RuleParameters { get; }
    public IProjectionToken Projection { get; }

    public EmptyRuleToken(
        string? @namespace,
        ICSharpTypeToken returnType,
        string name,
        CSharpParameterToken[] ruleParameters,
        IProjectionToken projection
    )
    {
        Namespace = @namespace;
        ReturnType = returnType;
        Name = name;
        RuleParameters = ruleParameters;
        Projection = projection;
    }

    public override string ToString()
    {
        return $"{ReturnType} {Name} = " +
               $"<none>" +
               $"{Projection}";
    }
}