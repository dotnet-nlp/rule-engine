using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class QuantifierToken : IToken
{
    public int Min { get; }
    public int? Max { get; }

    public QuantifierToken(int min, int? max)
    {
        Min = min;
        Max = max;
    }

    public override string ToString()
    {
        return Min switch
        {
            1 when Max == 1 => string.Empty,
            1 when Max is null => "+",
            0 when Max == 1 => "?",
            0 when Max is null => "*",
            _ => $"{{{Min.ToString()},{Max?.ToString()}}}"
        };
    }
}