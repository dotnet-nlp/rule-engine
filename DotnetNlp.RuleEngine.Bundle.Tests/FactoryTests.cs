using System.Collections.Generic;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Input;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Bundle.Tests;

public class FactoryTests
{
    [Test]
    [TestCaseSource(nameof(_sources))]
    public void Works(
        IReadOnlyDictionary<string, string> ruleSets,
        IReadOnlyDictionary<string, string> rules,
        IReadOnlyCollection<(string, string, object?)> ruleSetCases,
        IReadOnlyCollection<(string, string, bool)> ruleCases
    )
    {
        var ruleSpace = Factory.Create(
            ruleSets: ruleSets,
            rules: rules
        );

        foreach (var (ruleName, phrase, expectedResult) in ruleSetCases)
        {
            var cache = new RuleSpaceCache();

            var ruleMatchResultsCollection = ruleSpace[ruleName]
                .MatchAndProject(
                    new RuleInput(phrase.Split(' '), RuleSpaceArguments.Empty),
                    0,
                    RuleArguments.Empty,
                    cache
                );

            Assert.That(ruleMatchResultsCollection, Has.Count.EqualTo(1));

            var ruleMatchResult = ruleMatchResultsCollection.Single();

            Assert.That(ruleMatchResult.Result.Value, Is.EqualTo(expectedResult));
        }

        foreach (var (ruleName, phrase, expectedSuccess) in ruleCases)
        {
            var cache = new RuleSpaceCache();

            var ruleMatchResultsCollection = ruleSpace[ruleName]
                .MatchAndProject(
                    new RuleInput(phrase.Split(' '), RuleSpaceArguments.Empty),
                    0,
                    RuleArguments.Empty,
                    cache
                );

            Assert.That(ruleMatchResultsCollection, expectedSuccess ? Is.Not.Empty : Is.Empty);
        }
    }

    private static object?[][] _sources =
    {
        new object?[]
        {
            new Dictionary<string, string>()
            {
                {"number", "\r\nusing DotnetNlp.RuleEngine.Bundle;\r\n\r\nint Root = peg#($Number:n)# { return n; }\r\nint Number = peg#($Number_0:n_0|$Number_1:n_1|$Number_2:n_2|$Number_3:n_3|$Number_4:n_4|$Number_5:n_5|$Number_6:n_6|$Number_7:n_7|$Number_8:n_8|$Number_9:n_9)# { return Pick.OneOf(n_0, n_1, n_2, n_3, n_4, n_5, n_6, n_7, n_8, n_9); }\r\nint Number_0 = peg#(ноль)# => 0\r\nint Number_1 = peg#(один)# => 1\r\nint Number_2 = peg#(два)# => 2\r\nint Number_3 = peg#(три)# => 3\r\nint Number_4 = peg#(четыре)# => 4\r\nint Number_5 = peg#(пять)# => 5\r\nint Number_6 = peg#(шесть)# => 6\r\nint Number_7 = peg#(семь)# => 7\r\nint Number_8 = peg#(восемь)# => 8\r\nint Number_9 = peg#(девять)# => 9"},
            },
            new Dictionary<string, string>()
            {
                {"hi", "([привет~ здравствуй~]|[добрый доброе доброй доброго] [день дня вечер вечера утро утра ночь ночи])"},
            },
            new (string, string, object?)[]
            {
                ("number", "ноль", 0),
                ("number", "пять", 5),
                ("number", "девять", 9),
                ("number.Root", "ноль", 0),
                ("number.Root", "пять", 5),
                ("number.Root", "девять", 9),
                ("number.Number", "ноль", 0),
                ("number.Number", "пять", 5),
                ("number.Number", "девять", 9),
            },
            new []
            {
                ("hi", "привет", true),
                ("hi", "пока", false),
            },
        },
    };
}