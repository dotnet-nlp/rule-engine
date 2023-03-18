using System;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Equality;

public sealed class ChainedMemberAccessTokenEqualityComparer : IEqualityComparer<IChainedMemberAccessToken>
{
    public static readonly ChainedMemberAccessTokenEqualityComparer Instance = new();

    private ChainedMemberAccessTokenEqualityComparer()
    {
    }

    public bool Equals(IChainedMemberAccessToken? x, IChainedMemberAccessToken? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x switch
        {
            ChainedMemberAccessArgumentToken token => ChainedMemberAccessArgumentTokenEqualityComparer.Instance.Equals(token, y as ChainedMemberAccessArgumentToken),
            _ => throw new ArgumentOutOfRangeException(nameof(x)),
        };
    }

    public int GetHashCode(IChainedMemberAccessToken obj)
    {
        return obj switch
        {
            ChainedMemberAccessArgumentToken token => ChainedMemberAccessArgumentTokenEqualityComparer.Instance.GetHashCode(token),
            _ => throw new ArgumentOutOfRangeException(nameof(obj)),
        };
    }
}