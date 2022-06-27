using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class NerToken : IQuantifiableToken
{
    public string VariableName { get; }
    public string CallChain { get; }
    public IRuleArgumentToken[] Arguments { get; }

    public NerToken(string variableName, string callChain, IRuleArgumentToken[] arguments)
    {
        VariableName = variableName;
        CallChain = callChain;
        Arguments = arguments;
    }

    public override string ToString()
    {
        return
            $"<{VariableName} = {CallChain}({Arguments.Select(argument => argument.ToString()).JoinToString(", ")})>";
    }
}