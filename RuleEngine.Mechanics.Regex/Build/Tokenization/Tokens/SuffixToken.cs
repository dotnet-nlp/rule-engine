namespace RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class SuffixToken : ITerminalToken, ILiteralSetMemberToken
{
    public string Suffix { get; }

    public SuffixToken(string suffix)
    {
        Suffix = suffix;
    }

    public override string ToString()
    {
        return $"~{Suffix}";
    }
}