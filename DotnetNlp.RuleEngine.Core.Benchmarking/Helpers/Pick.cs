﻿using System;

namespace DotnetNlp.RuleEngine.Core.Benchmarking.Helpers;

/// <summary>
/// This class is used from within a rule projection.
/// </summary>
public static class Pick
{
    public static TValue OneOf<TValue>(params TValue?[] values) where TValue : struct
    {
        foreach (var value in values)
        {
            if (value != null)
            {
                return value.Value;
            }
        }

        throw new ArgumentException("One of values should be not equal to null", nameof(values));
    }

    public static TValue OneOf<TValue>(params TValue?[] values) where TValue : class
    {
        foreach (var value in values)
        {
            if (value is not null)
            {
                return value;
            }
        }

        throw new ArgumentException("One of values should be not equal to null", nameof(values));
    }
}