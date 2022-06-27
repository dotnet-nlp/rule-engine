namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class AnyLiteralToken : ITerminalToken
{
    public static readonly AnyLiteralToken Instance = new();

    private AnyLiteralToken()
    {
    }

    public override string ToString()
    {
        return ".";
    }
}