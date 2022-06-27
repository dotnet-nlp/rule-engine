using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

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