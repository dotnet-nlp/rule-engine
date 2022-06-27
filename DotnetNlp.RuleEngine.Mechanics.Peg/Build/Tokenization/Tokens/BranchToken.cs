using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;

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