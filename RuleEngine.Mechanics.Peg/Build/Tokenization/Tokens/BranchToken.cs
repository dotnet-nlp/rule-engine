using System.Linq;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

public sealed class BranchToken : IToken
{
    public BranchItemToken[] Items { get; }

    public BranchToken(BranchItemToken[] items)
    {
        Items = items;
    }

    public override string ToString()
    {
        return Items.Select(item => item.ToString()).JoinToString(" ");
    }
}