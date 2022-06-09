namespace RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class MarkerToken : IBranchItemToken
{
    public const char MarkerStart = '「';
    public const char MarkerEnd = '」';

    public string Marker { get; }

    public MarkerToken(string marker)
    {
        Marker = marker;
    }

    public override string ToString()
    {
        return $"{MarkerStart}{Marker}{MarkerEnd}";
    }
}