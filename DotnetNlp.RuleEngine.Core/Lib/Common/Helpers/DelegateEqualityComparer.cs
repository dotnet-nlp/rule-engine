using System;
using System.Collections;
using System.Collections.Generic;

namespace DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;

public sealed class DelegateEqualityComparer<TValue> : IEqualityComparer<TValue>, IEqualityComparer
{
    private readonly Func<TValue, TValue, bool> _compare;
    private readonly Func<TValue, int> _getHashCode;

    public DelegateEqualityComparer(Func<TValue, TValue, bool> compare, Func<TValue, int> getHashCode)
    {
        _compare = compare;
        _getHashCode = getHashCode;
    }

    bool IEqualityComparer.Equals(object? x, object? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is not TValue xValue)
        {
            return false;
        }

        if (y is not TValue yValue)
        {
            return false;
        }

        return Equals(xValue, yValue);
    }

    int IEqualityComparer.GetHashCode(object obj)
    {
        return obj is TValue token ? GetHashCode(token) : 0;
    }

    public bool Equals(TValue? x, TValue? y)
    {
        if (x is null != y is null)
        {
            return false;
        }

        if (x is null && y is null)
        {
            return true;
        }

        return _compare(x!, y!);
    }

    public int GetHashCode(TValue obj)
    {
        return _getHashCode(obj);
    }
}