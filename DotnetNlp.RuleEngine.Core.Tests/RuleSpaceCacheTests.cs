using System;
using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using DotnetNlp.RuleEngine.Core.Exceptions;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Core.Tests;

[TestFixture(TestOf = typeof(RuleSpaceCache))]
internal sealed class RuleSpaceCacheTests
{
    [Test]
    [TestCaseSource(nameof(SetsAndGets_Mixed))]
    public void SetsAndGets(
        bool projected,
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        KeyValuePair<string, object?>[] ruleArguments
    )
    {
        var result = new RuleMatchResultCollection(0);
        var cache = new RuleSpaceCache();

        cache.SetResult(projected, ruleId, inputSequence, nextSymbolIndex, ruleArguments, null, result);

        var resultFromCache = cache.GetResult(
            projected,
            ruleId,
            inputSequence.ToArray(),
            nextSymbolIndex,
            ruleArguments.ToArray(),
            null
        );

        Assert.IsNotNull(resultFromCache);
        Assert.AreSame(result, resultFromCache);
    }

    [Test]
    [TestCaseSource(nameof(DoesNotGetWithWrongKey_Mixed))]
    public void DoesNotGetWithWrongKey(
        (bool Projected, int RuleId, string[] InputSequence, int NextSymbolIndex, KeyValuePair<string, object?>[] RuleArguments) set,
        (bool Projected, int RuleId, string[] InputSequence, int NextSymbolIndex, KeyValuePair<string, object?>[] RuleArguments) get
    )
    {
        var result = new RuleMatchResultCollection(0);
        var cache = new RuleSpaceCache();

        cache.SetResult(set.Projected, set.RuleId, set.InputSequence, set.NextSymbolIndex, set.RuleArguments, null, result);

        var resultFromCache = cache.GetResult(
            get.Projected,
            get.RuleId,
            get.InputSequence,
            get.NextSymbolIndex,
            get.RuleArguments,
            null
        );

        Assert.IsNull(resultFromCache);
    }

    [Test]
    [TestCaseSource(nameof(ThrowsOnDuplicate_Mixed))]
    public void ThrowsOnDuplicate(
        bool projected,
        int ruleId,
        string[] inputSequence,
        int nextSymbolIndex,
        KeyValuePair<string, object?>[] ruleArguments
    )
    {
        var cache = new RuleSpaceCache();

        cache.SetResult(projected, ruleId, inputSequence, nextSymbolIndex, ruleArguments, null, new RuleMatchResultCollection(0));

        var exception = Assert.Throws<RuleMatchException>(
            () => cache.SetResult(projected, ruleId, inputSequence, nextSymbolIndex, ruleArguments, null, new RuleMatchResultCollection(0))
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
                true,
                1,
                new []{""},
                -1,
                Array.Empty<KeyValuePair<string, object?>>(),
            },
            new object?[]
            {
                false,
                2,
                new []{"один", "два"},
                0,
                new KeyValuePair<string, object?>[]
                {
                    new("foo", 42),
                    new("bar", TimeSpan.FromDays(1)),
                    new("baz", "bazz"),
                },
            },
        };
    }

    #endregion

    #region Sources_DoesNotGetWithWrongKey

    public static (bool, int, string[], int, KeyValuePair<string, object?>[])[][] DoesNotGetWithWrongKey_Mixed()
    {
        return new []
        {
            new []
            {
                (
                    true,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
                (
                    false,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
            },
            new []
            {
                (
                    false,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
                (
                    true,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
            },
            new []
            {
                (
                    true,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
                (
                    true,
                    2,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
            },
            new []
            {
                (
                    true,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
                (
                    true,
                    1,
                    new []{"один"},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
            },
            new []
            {
                (
                    false,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
                (
                    false,
                    1,
                    new []{""},
                    0,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
            },
            new []
            {
                (
                    false,
                    1,
                    new []{""},
                    -1,
                    Array.Empty<KeyValuePair<string, object?>>()
                ),
                (
                    false,
                    1,
                    new []{""},
                    -1,
                    new KeyValuePair<string, object?>[]
                    {
                        new("foo", "bar"),
                    }
                ),
            },
        };
    }

    #endregion

    #region Sources_ThrowsOnDuplicate

    public static object?[][] ThrowsOnDuplicate_Mixed()
    {
        return new[]
        {
            new object?[]
            {
                true,
                1,
                new []{""},
                -1,
                Array.Empty<KeyValuePair<string, object?>>(),
            },
            new object?[]
            {
                false,
                2,
                new []{"один", "два"},
                0,
                new KeyValuePair<string, object?>[]
                {
                    new("foo", 42),
                    new("bar", TimeSpan.FromDays(1)),
                    new("baz", "bazz"),
                },
            },
        };
    }

    #endregion

    #endregion
}