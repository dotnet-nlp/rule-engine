using System.Collections.Generic;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Tests.Helpers;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Core.Tests;

[TestFixture(TestOf = typeof(RuleSpace))]
internal sealed class RuleSpaceTests
{
    [Test]
    [TestCaseSource(nameof(Prunes_All))]
    public void Prunes(
        IReadOnlyDictionary<string, string> rules,
        IReadOnlyDictionary<string, string> ruleSets,
        IReadOnlyCollection<string> rulesToPreserve,
        IReadOnlyCollection<string> expectedRemovedRuleKeys
    )
    {
        var ruleSpaceSource = new RuleSpaceSource(
            new RuleSpaceFactory(
                new []
                {
                    StaticResources.RegexMechanics(),
                    StaticResources.PegMechanics(),
                }
            ),
            rules,
            ruleSets
        );

        var removedRuleKeys = ruleSpaceSource.RuleSpace.Prune(rulesToPreserve);

        CollectionAssert.AreEquivalent(expectedRemovedRuleKeys, removedRuleKeys);
    }

    #region Sources

    #region Sources_Prunes

    public static object?[][] Prunes_All()
    {
        return new[]
        {
            new object?[]
            {
                new Dictionary<string, string>()
                {
                    {"preserve_1", "(<dependency_1> <dependency_2>)"},
                    {"preserve_2", "(.)"},
                    {"not_preserve_1", "(<dependency_1> <dependency_2> <dependency_3>)"},
                    {"not_preserve_2", "(<dependency_2> <dependency_3> <dependency_4>)"},
                    {"dependency_1", "(<dependency_5>)"},
                    {"dependency_2", "(.)"},
                    {"dependency_3", "(.)"},
                    {"dependency_4", "(.)"},
                    {"dependency_5", "(<a = ner_1_or_2()> <dependency_6>)"},
                    {"dependency_6", "(<a = ner_3_or_4.three()>)"},
                },
                new Dictionary<string, string>()
                {
                    {
                        "ner_1_or_2",
                        "int Root = peg#($one:one|$two:two)# { return (one ?? two).Value; }\r\nint one = peg#(один)# => 1\r\nint two = peg#(два)# => 2"
                    },
                    {
                        "ner_3_or_4",
                        "int three = peg#(три)# => 3\r\nint four = peg#(четыре)# => 4"
                    },
                },
                new []
                {
                    "preserve_1",
                    "preserve_2",
                },
                new []
                {
                    "not_preserve_1",
                    "not_preserve_2",
                    "dependency_3",
                    "dependency_4",
                    "ner_1_or_2.Root",
                    "ner_3_or_4.four",
                },
            },
        };
    }

    #endregion

    #endregion
}