namespace RuleEngine.Core.Build.Tokenization.Tokens.Arguments;

public sealed class RuleDefaultArgumentToken : IRuleArgumentToken
{
    public static readonly RuleDefaultArgumentToken Instance = new();

    private RuleDefaultArgumentToken()
    {
    }

    public override string ToString()
    {
        return "default";
    }
}