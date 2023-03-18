using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Equality;

public sealed class ChainedMemberAccessArgumentTokenEqualityComparer : IEqualityComparer<ChainedMemberAccessArgumentToken>
{
    public static readonly ChainedMemberAccessArgumentTokenEqualityComparer Instance = new();

    private ChainedMemberAccessArgumentTokenEqualityComparer()
    {
    }

    public bool Equals(ChainedMemberAccessArgumentToken? x, ChainedMemberAccessArgumentToken? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
        {
            return false;
        }

        return x.CallChain.SequenceEqual(y.CallChain);
    }

    public int GetHashCode(ChainedMemberAccessArgumentToken obj)
    {
        return obj.CallChain.GetSequenceHashCode<string>();
    }
}