using System.Collections.Immutable;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result;
using Moq;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Core.Tests;

[TestFixture(TestOf = typeof(CachingRuleMatcher))]
internal sealed class CachingRuleMatcherTests
{
    [Test]
    [TestCase(300, new string[0], 42)]
    [TestCase(42, new [] {"foo"}, 300)]
    public void WorksWithEmptyCache(int matcherId, string[] sequence, int firstSymbolIndex)
    {
        var ruleInput = new RuleInput(
            sequence,
            new RuleSpaceArguments(ImmutableDictionary<string, object?>.Empty)
        );
        var ruleArguments = new RuleArguments(ImmutableDictionary<string, object?>.Empty);
        var mockedResults = new RuleMatchResultCollection(0);

        var nestedRuleMatcherMock = new Mock<IRuleMatcher>();
        var cacheMock = new Mock<IRuleSpaceCache>();

        nestedRuleMatcherMock
            .Setup(nestedRuleMatcher => nestedRuleMatcher.MatchAndProject(ruleInput, firstSymbolIndex, ruleArguments, cacheMock.Object))
            .Returns(mockedResults);

        var ruleMatcher = new CachingRuleMatcher(
            matcherId,
            nestedRuleMatcherMock.Object
        );

        var results = ruleMatcher.MatchAndProject(
            ruleInput,
            firstSymbolIndex,
            ruleArguments,
            cacheMock.Object
        );

        nestedRuleMatcherMock.Verify(nestedRuleMatcher => nestedRuleMatcher.MatchAndProject(ruleInput, firstSymbolIndex, ruleArguments, cacheMock.Object), Times.Once);
        nestedRuleMatcherMock.VerifyNoOtherCalls();

        cacheMock.Verify(cache => cache.GetResult(matcherId, ruleInput.Sequence, firstSymbolIndex, ruleArguments.Values), Times.Once);
        cacheMock.Verify(cache => cache.SetResult(matcherId, ruleInput.Sequence, firstSymbolIndex, ruleArguments.Values, mockedResults), Times.Once);
        cacheMock.VerifyNoOtherCalls();

        Assert.AreSame(mockedResults, results);
    }

    [Test]
    [TestCase(300, new string[0], 42)]
    [TestCase(42, new [] {"foo"}, 300)]
    public void WorksWithFilledCache(int matcherId, string[] sequence, int firstSymbolIndex)
    {
        var ruleInput = new RuleInput(
            sequence,
            new RuleSpaceArguments(ImmutableDictionary<string, object?>.Empty)
        );
        var ruleArguments = new RuleArguments(ImmutableDictionary<string, object?>.Empty);
        var mockedResults = new RuleMatchResultCollection(0);

        var nestedRuleMatcherMock = new Mock<IRuleMatcher>();
        var cacheMock = new Mock<IRuleSpaceCache>();

        cacheMock
            .Setup(cache => cache.GetResult(matcherId, ruleInput.Sequence, firstSymbolIndex, ruleArguments.Values))
            .Returns(mockedResults);

        var ruleMatcher = new CachingRuleMatcher(
            matcherId,
            nestedRuleMatcherMock.Object
        );

        var results = ruleMatcher.MatchAndProject(
            ruleInput,
            firstSymbolIndex,
            ruleArguments,
            cacheMock.Object
        );

        nestedRuleMatcherMock.VerifyNoOtherCalls();

        cacheMock.Verify(cache => cache.GetResult(matcherId, ruleInput.Sequence, firstSymbolIndex, ruleArguments.Values), Times.Once);
        cacheMock.VerifyNoOtherCalls();

        Assert.AreSame(mockedResults, results);
    }
}