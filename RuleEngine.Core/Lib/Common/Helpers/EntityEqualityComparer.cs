using System;
using System.Collections.Generic;

namespace RuleEngine.Core.Lib.Common.Helpers;

public sealed class EntityEqualityComparer<TValue, TEntity> : IEqualityComparer<TValue>
    where TEntity : notnull
{
    private readonly Func<TValue, TEntity> _entityGetter;

    public EntityEqualityComparer(Func<TValue, TEntity> entityGetter)
    {
        _entityGetter = entityGetter;
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

        return _entityGetter(x!).Equals(_entityGetter(y!));
    }

    public int GetHashCode(TValue obj)
    {
        return _entityGetter(obj!).GetHashCode();
    }
}