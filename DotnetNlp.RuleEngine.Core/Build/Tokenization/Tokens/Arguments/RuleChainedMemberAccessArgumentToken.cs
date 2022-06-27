using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;

public sealed class RuleChainedMemberAccessArgumentToken : IRuleArgumentToken
{
    public string[] CallChain { get; }

    public RuleChainedMemberAccessArgumentToken(string[] callChain)
    {
        CallChain = callChain;
    }

    public override string ToString()
    {
        return CallChain.JoinToString(".");
    }
}