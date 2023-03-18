namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;

public interface IChainedMemberAccessToken
{
    string[] CallChain { get; }

    string ToString();
}