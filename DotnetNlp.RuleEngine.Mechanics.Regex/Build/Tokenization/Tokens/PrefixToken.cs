namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

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