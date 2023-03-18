using System;
using System.Collections;
using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens.Arguments;

namespace DotnetNlp.RuleEngine.Core.Build.Tokenization.Equality;

public sealed class RuleArgumentTokenEqualityComparer : IEqualityComparer<IRuleArgumentToken>, IEqualityComparer
{
    public static readonly RuleArgumentTokenEqualityComparer Instance = new();

    private RuleArgumentTokenEqualityComparer()
    {
    }

    bool IEqualityComparer.Equals(object? x, object? y)
    {
        return Equals(x as IRuleArgumentToken, y as IRuleArgumentToken);
    }

    int IEqualityComparer.GetHashCode(object obj)
    {
        return obj is IRuleArgumentToken token ? GetHashCode(token) : 0;
    }

    public bool Equals(IRuleArgumentToken? x, IRuleArgumentToken? y)
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
            RuleDefaultArgumentToken => true,
            ChainedMemberAccessArgumentToken token => ChainedMemberAccessArgumentTokenEqualityComparer.Instance.Equals(token, y as ChainedMemberAccessArgumentToken),
            _ => throw new ArgumentOutOfRangeException(nameof(x)),
        };
    }

    public int GetHashCode(IRuleArgumentToken obj)
    {
        return obj switch
        {
            RuleDefaultArgumentToken => typeof(RuleDefaultArgumentToken).GetHashCode(),
            ChainedMemberAccessArgumentToken token => ChainedMemberAccessArgumentTokenEqualityComparer.Instance.GetHashCode(token),
            _ => throw new ArgumentOutOfRangeException(nameof(obj)),
        };
    }
}