using System.Linq;
using RuleEngine.Core.Build.Tokenization.Tokens;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class PegGroupToken : IQuantifiableToken, IPatternToken
{
    public BranchToken[] Branches { get; }

    public PegGroupToken(BranchToken[] branches)
    {
        Branches = branches;
    }

    public override string ToString()
    {
        return $"({Branches.Select(branch => branch.ToString()).JoinToString(" | ")})";
    }
}