using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;

public sealed class ChainedMemberAccessArgumentToken : IRuleArgumentToken, IChainedMemberAccessToken
{
    public string[] CallChain { get; }

    public ChainedMemberAccessArgumentToken(string[] callChain)
    {
        CallChain = callChain;
    }

    public override string ToString()
    {
        return CallChain.JoinToString(".");
    }
}