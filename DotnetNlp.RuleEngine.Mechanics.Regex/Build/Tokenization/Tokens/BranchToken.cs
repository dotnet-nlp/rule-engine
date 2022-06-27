using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;

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