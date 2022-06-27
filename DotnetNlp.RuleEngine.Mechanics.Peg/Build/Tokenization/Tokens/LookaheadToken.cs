using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

// todo [realtime performance] introduce two instances and make constructor private
public sealed class LookaheadToken : IToken
{
    public bool IsNegative { get; }

    public LookaheadToken(bool isNegative)
    {
        IsNegative = isNegative;
    }

    public override string ToString()
    {
        return IsNegative ? "!" : "&";
    }
}