namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class PrefixToken : ITerminalToken, ILiteralSetMemberToken
{
    public string Prefix { get; }

    public PrefixToken(string prefix)
    {
        Prefix = prefix;
    }

    public override string ToString()
    {
        return $"{Prefix}~";
    }
}