using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class CSharpChainedMemberAccessToken : IToken
{
    public string[] Value { get; }

    public CSharpChainedMemberAccessToken(string[] value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.JoinToString(".");
    }
}