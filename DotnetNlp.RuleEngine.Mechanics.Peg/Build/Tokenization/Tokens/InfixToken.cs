namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class InfixToken : ITerminalToken, ILiteralSetMemberToken
{
    public string Infix { get; }

    public InfixToken(string infix)
    {
        Infix = infix;
    }

    public override string ToString()
    {
        return $"~{Infix}~";
    }
}