using System.Linq;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;

public sealed class TupleCSharpTypeToken : ICSharpTypeToken
{
    public CSharpTupleItemToken[] Items { get; }

    public TupleCSharpTypeToken(CSharpTupleItemToken[] items)
    {
        Items = items;
    }

    public override string ToString()
    {
        return $"({Items.Select(item => item.ToString()).JoinToString(", ")})";
    }
}