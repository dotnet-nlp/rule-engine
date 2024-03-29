﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Exceptions;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Core.Tests;

[TestFixture(TestOf = typeof(RuleSpaceCache))]
internal sealed class RuleSpaceCacheTests
{
    [Test]
    [TestCaseSource(nameof(SetsAndGets_Mixed))]
    public void SetsAndGets(
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        IReadOnlyDictionary<string, object?> ruleArguments
    )
    {
        var result = new RuleMatchResultCollection(0);
        var cache = new RuleSpaceCache();

        cache.SetResult(ruleId, inputSequence, nextSymbolIndex, ruleArguments, result);

        var resultFromCache = cache.GetResult(
            ruleId,
            inputSequence.ToArray(),
            nextSymbolIndex,
            ruleArguments.ToDictionary()
        );

        Assert.IsNotNull(resultFromCache);
        Assert.AreSame(result, resultFromCache);
    }

    [Test]
    [TestCaseSource(nameof(DoesNotGetWithWrongKey_Mixed))]
    public void DoesNotGetWithWrongKey(
        (int RuleId, string[] InputSequence, int NextSymbolIndex, IReadOnlyDictionary<string, object?> RuleArguments) set,
        (int RuleId, string[] InputSequence, int NextSymbolIndex, IReadOnlyDictionary<string, object?> RuleArguments) get
    )
    {
        var result = new RuleMatchResultCollection(0);
        var cache = new RuleSpaceCache();

        cache.SetResult(set.RuleId, set.InputSequence, set.NextSymbolIndex, set.RuleArguments, result);

        var resultFromCache = cache.GetResult(
            get.RuleId,
            get.InputSequence,
            get.NextSymbolIndex,
            get.RuleArguments
        );

        Assert.IsNull(resultFromCache);
    }

    [Test]
    [TestCaseSource(nameof(ThrowsOnDuplicate_Mixed))]
    public void ThrowsOnDuplicate(
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        IReadOnlyDictionary<string, object?> ruleArguments
    )
    {
        var cache = new RuleSpaceCache();

        cache.SetResult(ruleId, inputSequence, nextSymbolIndex, ruleArguments, new RuleMatchResultCollection(0));

        var exception = Assert.Throws<RuleMatchException>(
            () => cache.SetResult(ruleId, inputSequence, nextSymbolIndex, ruleArguments, new RuleMatchResultCollection(0))
        );

        Assert.AreEqual("Error during adding rule match result to cache.", exception!.Message);
    }

    #region Sources

    #region Sources_SetsAndGets

    public static object?[][] SetsAndGets_Mixed()
    {
        return new[]
        {
            new object?[]
            {
                1,
                new []{""},
                -1,
                ImmutableDictionary<string, object?>.Empty,
            },
            new object?[]
            {
                2,
                new []{"один", "два"},
                0,
                new Dictionary<string, object?>
                {
                    {"foo", 42},
                    {"bar", TimeSpan.FromDays(1)},
                    {"baz", "bazz"},
                },
            },
        };
    }

    #endregion

    #region Sources_DoesNotGetWithWrongKey

    public static (int, string[], int, IReadOnlyDictionary<string, object?>)[][] DoesNotGetWithWrongKey_Mixed()
    {
        return new []
        {
            new []
            {
                (
                    1,
                    new []{""},
                    -1,
                    (IReadOnlyDictionary<string, object?>) ImmutableDictionary<string, object?>.Empty
                ),
                (
                    2,
                    new []{""},
                    -1,
                    (IReadOnlyDictionary<string, object?>) ImmutableDictionary<string, object?>.Empty
                ),
            },
            new []
            {
                (
                    1,
                    new []{""},
                    -1,
                    (IReadOnlyDictionary<string, object?>) ImmutableDictionary<string, object?>.Empty
                ),
                (
                    1,
                    new []{"один"},
                    -1,
                    (IReadOnlyDictionary<string, object?>) ImmutableDictionary<string, object?>.Empty
                ),
            },
            new []
            {
                (
                    1,
                    new []{""},
                    -1,
                    (IReadOnlyDictionary<string, object?>) ImmutableDictionary<string, object?>.Empty
                ),
                (
                    1,
                    new []{""},
                    0,
                    (IReadOnlyDictionary<string, object?>) ImmutableDictionary<string, object?>.Empty
                ),
            },
            new []
            {
                (
                    1,
                    new []{""},
                    -1,
                    (IReadOnlyDictionary<string, object?>) ImmutableDictionary<string, object?>.Empty
                ),
                (
                    1,
                    new []{""},
                    -1,
                    (IReadOnlyDictionary<string, object?>) new Dictionary<string, object?>
                    {
                        {"foo", "bar"},
                    }
                ),
            },
        };
    }

    #endregion

    #region Sources_Overrides

    public static object?[][] ThrowsOnDuplicate_Mixed()
    {
        return new[]
        {
            new object?[]
            {
                1,
                new []{""},
                -1,
                ImmutableDictionary<string, object?>.Empty,
            },
            new object?[]
            {
                2,
                new []{"один", "два"},
                0,
                new Dictionary<string, object?>
                {
                    {"foo", 42},
                    {"bar", TimeSpan.FromDays(1)},
                    {"baz", "bazz"},
                },
            },
        };
    }

    #endregion

    #endregion
}