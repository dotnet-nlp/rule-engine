namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class LiteralToken : ITerminalToken, ILiteralSetMemberToken
{
    public string Literal { get; }

    public LiteralToken(string literal)
    {
        Literal = literal;
    }

    public override string ToString()
    {
        return $"{Literal}";
    }
}