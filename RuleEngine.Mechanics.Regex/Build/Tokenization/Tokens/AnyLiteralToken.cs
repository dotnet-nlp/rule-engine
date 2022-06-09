namespace RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

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