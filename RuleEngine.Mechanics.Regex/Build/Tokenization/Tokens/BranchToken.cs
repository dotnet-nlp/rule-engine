using System.Linq;
using RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using RuleEngine.Core.Lib.Common.Helpers;

namespace RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

public sealed class BranchToken : IToken
{
    public IBranchItemToken[] Items { get; }

    public BranchToken(IBranchItemToken[] items)
    {
        Items = items;
    }

    public override string ToString()
    {
        return Items.Select(item => item.ToString()).JoinToString(" ");
    }
}