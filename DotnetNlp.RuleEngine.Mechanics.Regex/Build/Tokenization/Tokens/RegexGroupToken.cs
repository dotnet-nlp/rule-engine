using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class RegexGroupToken : IQuantifiableToken, IPatternToken
{
    public BranchToken[] Branches { get; }

    public RegexGroupToken(BranchToken[] branches)
    {
        Branches = branches;
    }

    public override string ToString()
    {
        return $"({Branches.Select(branch => branch.ToString()).JoinToString(" | ")})";
    }
}