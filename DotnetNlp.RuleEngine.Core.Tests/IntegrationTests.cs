using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotnetNlp.RuleEngine.Core.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Evaluation;
using DotnetNlp.RuleEngine.Core.Evaluation.Cache;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Projection.Arguments;
using DotnetNlp.RuleEngine.Core.Evaluation.Rule.Result.SelectionStrategy;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Assemblies;
using DotnetNlp.RuleEngine.Core.Lib.CodeAnalysis.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Core.Lib.Common;
using DotnetNlp.RuleEngine.Core.Lib.Common.Helpers;
using DotnetNlp.RuleEngine.Core.Tests.Fixtures;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Peg.Build.Tokenization.Tokens;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.InputProcessing.Automaton.Optimization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization;
using DotnetNlp.RuleEngine.Mechanics.Regex.Build.Tokenization.Tokens;
using NUnit.Framework;

namespace DotnetNlp.RuleEngine.Core.Tests;

[TestFixture]
internal sealed class IntegrationTests
{
    private IResultSelectionStrategy? _bestReferenceSelectionStrategy;
    private RuleSpaceArguments? _ruleSpaceArguments;

    private IRuleSpace? _ruleSpace;

    [SetUp]
    public void OneTimeSetUp()
    {
        _bestReferenceSelectionStrategy = new CombinedStrategy(
            new IResultSelectionStrategy[]
            {
                new MaxExplicitSymbolsStrategy(),
                new MaxProgressStrategy(),
            }
        );

        _ruleSpaceArguments = new RuleSpaceArguments(
            new Dictionary<string, object?>
            {
                {
                    "parameters",
                    new DummyParameters(
                        25,
                        new DummySaltParameters("eto_sol", 3),
                        new DummySaltNullParameters()
                    )
                },
                {
                    "parametersDigitsSalt",
                    new DummySaltParameters("eto_sol", 3)
                },
                {
                    "rule_space_test_string_argument",
                    "some fancy string"
                },
                {
                    "rule_space_test_int_argument",
                    42
                },
                {
                    "rule_space_test_enumerable_argument",
                    Enumerable.Empty<object>().Append(420).ToList()
                },
                {
                    "mutable_parameters",
                    new DummyMutableParameters()
                },
            }
        );

        var stringInterner = new StringInterner();
        var errorIndexHelper = new ErrorIndexHelper(Environment.NewLine);

        var factory = new RuleSpaceFactory(
            new []
            {
                new MechanicsDescription("peg", new LoopBasedPegPatternTokenizer(stringInterner, errorIndexHelper), new PegProcessorFactory(_bestReferenceSelectionStrategy), typeof(PegGroupToken)),
                new MechanicsDescription("regex", new LoopBasedRegexPatternTokenizer(stringInterner, errorIndexHelper), new RegexProcessorFactory(OptimizationLevel.Max), typeof(RegexGroupToken)),
            }
        );

        var phrases = new Dictionary<string, string[]>
        {
            // todo [tests] write separate test for these cases
            // {
            //     "ReferenceForNotExistingRule",
            //     new []
            //     {
            //         "<NotExistingRule>"
            //     }
            // },
            // {
            //     "NerFromNotExistingRule",
            //     new []
            //     {
            //         "<a = ner.NotExistingRule()>"
            //     }
            // },
            {
                "Test2",
                new []
                {
                    ".* тест .*",
                }
            },
            {
                "Hello",
                new []
                {
                    "привет",
                    "добрый день",
                    "здравствуй~",
                    "доброго времени суток",
                    "я? приветствую [вас тебя]?",
                }
            },
            {
                "GoodBye",
                new []
                {
                    "пока",
                    "до свидания",
                    "прощай~",
                    "аривидерчи",
                }
            },
            {
                "GoodMorning",
                new []
                {
                    "доброе [утро утречко]",
                    "с .{0,3} утром",
                }
            },
            {
                "GoodMorning2",
                new []
                {
                    "гутен морген",
                }
            },
            {
                "GoodEvening",
                new []
                {
                    "добрый вечер~",
                    "с добрым [вечером вечерком вечерочком]",
                }
            },
            {
                "NumberComparison",
                new []
                {
                    "число <a = ner.digit()> больше чем число <b = ner.digit()>",
                }
            },
            {
                "NumberComparisonWithGreeting",
                new []
                {
                    "<greeting = patterns.Hello()> число <digit = ner.digit()>",
                }
            },
            {
                "Preferable",
                new []
                {
                    "желательно",
                    "хотелось бы",
                }
            },
            {
                "IfPossible",
                new []
                {
                    "по возможности",
                }
            },
            {
                "Hours",
                new []
                {
                    "[час часа часов]",
                }
            },
            {
                "Time",
                new []
                {
                    "это [было будет] <when = sdn.times()>",
                }
            },
            {
                "Maybe",
                new []
                {
                    "возможно",
                }
            },
            {
                "ReferenceToRuleFromIncludeWithReferenceToRuleFromOtherInclude",
                new []
                {
                    "(<include4.patterns.rule1>)",
                }
            },
            {
                "ReferenceToRuleWithArguments",
                new []
                {
                    "(<ner.any_arguments(mutable_parameters.String, mutable_parameters.Int)>)",
                }
            },
        };

        var sdn = new Dictionary<string, string>
        {
            {
                "times",
                @"
using DotnetNlp.RuleEngine.Core.Tests.Helpers;

string Root = peg#($Timing:timing $patterns.Maybe?)#
{
    return timing;
}

string Timing = peg#($after:after|$before:before)#
{
    return Pick.OneOf(after, before);
}

string after = peg#(через $ner.digit:hours $patterns.Hours)# { return hours > 2 ? ""не скоро"" : ""скоро""; }

string before = peg#($ner.digit:hours $patterns.Hours назад)# { return hours > 2 ? ""давно"" : ""недавно""; }
"
            },
            {
                "digit",
                @"
using DotnetNlp.RuleEngine.Core.Tests.Helpers;

int Root = peg#($Number:n)# { return n; }

int Number = peg#($Number_0:n_0|$Number_1:n_1|$Number_2:n_2|$Number_3:n_3|$Number_4:n_4|$Number_5:n_5|$Number_6:n_6|$Number_7:n_7|$Number_8:n_8|$Number_9:n_9)# { return Pick.OneOf(n_0, n_1, n_2, n_3, n_4, n_5, n_6, n_7, n_8, n_9); }
int Number_0 = peg#(ноль)# { return 0; }
int Number_1 = peg#(один)# { return 1; }
int Number_2 = peg#(два)# { return 2; }
int Number_3 = peg#(три)# { return 3; }
int Number_4 = peg#(четыре)# { return 4; }
int Number_5 = peg#(пять)# { return 5; }
int Number_6 = peg#(шесть)# { return 6; }
int Number_7 = peg#(семь)# { return 7; }
int Number_8 = peg#(восемь)# { return 8; }
int Number_9 = peg#(девять)# { return 9; }"
            },
            {
                "digits_salted_sum",
                @"
int Root = peg#($SaltedSum:s)# { return s; }

int SaltedSum = peg#($sdn.digit:a плюс $sdn.digit:b)# { return a + b + parameters.Salt; }
"
            },
            {
                "rule_space_args_test",
                @"
string StringValue = peg#(.:word)#
{
    return $""{word}_{rule_space_test_string_argument}_{rule_space_test_int_argument}_{string.Join("" "", rule_space_test_enumerable_argument)}"";
}"
            },
            {
                "rule_args_test",
                @"
string PegDefaultArgs = peg#($ner.digit_with_salt:word)#
{
    return word;
}

string PegDefaultRepeat = peg#($ner.digit_with_salt(parameters.DigitsSalt.Salt, default):word)#
{
    return word;
}

string PegDefaultSalt = peg#($ner.digit_with_salt(default, parameters.DigitsSalt.RepeatTimes):word)#
{
    return word;
}

string PegDefinedArgs = peg#($ner.digit_with_salt(parameters.DigitsSalt.Salt, parameters.DigitsSalt.RepeatTimes):word)#
{
    return word;
}

string RegexDefaultArgs = regex#(<word = ner.digit_with_salt(default, default)>)#
{
    return word;
}

string RegexDefaultRepeat = regex#(<word = ner.digit_with_salt(parametersDigitsSalt.Salt, default)>)#
{
    return word;
}

string RegexDefaultSalt = regex#(<word = ner.digit_with_salt(default, parametersDigitsSalt.RepeatTimes)>)#
{
    return word;
}

string RegexDefinedArgs = regex#(<word = ner.digit_with_salt(parametersDigitsSalt.Salt, parametersDigitsSalt.RepeatTimes)>)#
{
    return word;
}"
            },
            {
                "multivalued_ners",
                @"
int Foo = peg#($ner.number_line:number)#
{
    return number;
}"
            },
        };

        var rulesByName = phrases
            .MapValue(patterns => $"({patterns.Select<string, string>(pattern => $"({pattern})").JoinToString(" | ")})")
            .MapValue(pattern => factory.PatternTokenizers["regex"].Tokenize(pattern, "patterns", false))
            .MapValue(
                (ruleName, patternToken) => new RuleToken(
                    "patterns",
                    MatchedInputBasedProjectionToken.ReturnType,
                    ruleName,
                    Array.Empty<CSharpParameterToken>(),
                    "regex",
                    patternToken,
                    MatchedInputBasedProjectionToken.Instance
                )
            )
            .SelectValues()
            .ToArray();

        var sdnRuleSetsByName = sdn
            .MapValue((ruleSetName, ruleSet) => factory.RuleSetTokenizer.Tokenize(ruleSet, $"sdn.{ruleSetName}", true))
            .SelectValues()
            .ToArray();

        var include1 = CreateIncludedRuleSpace1();
        var include2 = CreateIncludedRuleSpace2();
        var include3 = CreateIncludedRuleSpace3();
        var include4 = CreateIncludedRuleSpace4();

        _ruleSpace = factory.Create(
            "script",
            sdnRuleSetsByName,
            rulesByName,
            new [] { typeof(DummyStaticNerContainer) }
                .Select(container => factory.StaticRuleFactory.ConvertStaticRuleContainerToRuleMatchers(container))
                .Merge(),
            new []
            {
                include1,
                include2,
                include4,
            },
            _ruleSpaceArguments.Values.MapValue(argument => argument!.GetType()).ToDictionary(),
            LoadedAssembliesProvider.Instance
        );

        IRuleSpace CreateIncludedRuleSpace1()
        {
            var includeSdn = new Dictionary<string, string>()
            {
                { "number1to9", Numbers0To9Ner.Instance.Definition },
            };

            var includePatterns = new Dictionary<string, string>()
            {
                { "rule1", "(<rule2>)" },
                { "rule2", "(<rule3>)" },
                { "rule3", "(.*)" },
                { "rule4", "(.*)" },
                { "rule5", "(.*)" },
                { "rule6", "(.*)" },
            };

            return factory.Create(
                "include1",
                includeSdn
                    .MapValue(
                        (ruleSetName, ruleSet) => factory.RuleSetTokenizer.Tokenize(ruleSet, $"sdn.{ruleSetName}", false)
                    )
                    .SelectValues()
                    .ToArray(),
                includePatterns
                    .MapValue(
                        (ruleName, value) => new RuleToken(
                            "patterns",
                            MatchedInputBasedProjectionToken.ReturnType,
                            ruleName,
                            Array.Empty<CSharpParameterToken>(),
                            "regex",
                            factory.PatternTokenizers["regex"].Tokenize(value, "patterns", false),
                            MatchedInputBasedProjectionToken.Instance
                        )
                    )
                    .SelectValues()
                    .ToArray(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                Array.Empty<IRuleSpace>(),
                ImmutableDictionary<string, Type>.Empty,
                LoadedAssembliesProvider.Instance
            );
        }

        IRuleSpace CreateIncludedRuleSpace2()
        {
            var includeSdn = new Dictionary<string, string>()
            {
                { "digit1to9", Digits0To9Ner.Instance.Definition },
            };

            var includePatterns = new Dictionary<string, string>()
            {
                { "rule1", "(<include1.patterns.rule5>)" },
            };

            return factory.Create(
                "include2",
                includeSdn
                    .MapValue(
                        (ruleSetName, ruleSet) => factory.RuleSetTokenizer.Tokenize(ruleSet, $"sdn.{ruleSetName}", false)
                    )
                    .SelectValues()
                    .ToArray(),
                includePatterns
                    .MapValue(
                        (ruleName, value) => new RuleToken(
                            "patterns",
                            MatchedInputBasedProjectionToken.ReturnType,
                            ruleName,
                            Array.Empty<CSharpParameterToken>(),
                            "regex",
                            factory.PatternTokenizers["regex"].Tokenize(value, "patterns", false),
                            MatchedInputBasedProjectionToken.Instance
                        )
                    )
                    .SelectValues()
                    .ToArray(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                new []
                {
                    include1,
                },
                ImmutableDictionary<string, Type>.Empty,
                LoadedAssembliesProvider.Instance
            );
        }

        IRuleSpace CreateIncludedRuleSpace3()
        {
            var includeSdn = new Dictionary<string, string>()
            {
                { "doctor", DoctorsNer.Instance.Definition },
            };

            var includePatterns = new Dictionary<string, string>()
            {
                { "rule1", "(<include1.patterns.rule5>)" },
                { "rule2", "(<include1.patterns.rule6>)" },
                { "rule3", "(<include2.patterns.rule1>)" },
            };

            return factory.Create(
                "include3",
                includeSdn
                    .MapValue(
                        (ruleSetName, ruleSet) => factory.RuleSetTokenizer.Tokenize(ruleSet, $"sdn.{ruleSetName}", false)
                    )
                    .SelectValues()
                    .ToArray(),
                includePatterns
                    .MapValue(
                        (ruleName, value) => new RuleToken(
                            "patterns",
                            MatchedInputBasedProjectionToken.ReturnType,
                            ruleName,
                            Array.Empty<CSharpParameterToken>(),
                            "regex",
                            factory.PatternTokenizers["regex"].Tokenize(value, "patterns", false),
                            MatchedInputBasedProjectionToken.Instance
                        )
                    )
                    .SelectValues()
                    .ToArray(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                new []
                {
                    include1,
                    include2,
                },
                ImmutableDictionary<string, Type>.Empty,
                LoadedAssembliesProvider.Instance
            );
        }

        IRuleSpace CreateIncludedRuleSpace4()
        {
            var includePatterns = new Dictionary<string, string>()
            {
                { "rule1", "(<include3.patterns.rule2>|<include3.patterns.rule3>)" },
            };

            return factory.Create(
                "include4",
                Array.Empty<RuleSetToken>(),
                includePatterns
                    .MapValue(
                        (ruleName, value) => new RuleToken(
                            "patterns",
                            MatchedInputBasedProjectionToken.ReturnType,
                            ruleName,
                            Array.Empty<CSharpParameterToken>(),
                            "regex",
                            factory.PatternTokenizers["regex"].Tokenize(value, "patterns", false),
                            MatchedInputBasedProjectionToken.Instance
                        )
                    )
                    .SelectValues()
                    .ToArray(),
                ImmutableDictionary<string, IRuleMatcher>.Empty,
                new []
                {
                    include3,
                },
                ImmutableDictionary<string, Type>.Empty,
                LoadedAssembliesProvider.Instance
            );
        }
    }

    [Test]
    [TestCase("ner.digit", "четыре", true, 4)]
    [TestCase("ner.digit", "ноль", true, 0)]
    [TestCase("ner.digit", "сто", false)]
    [TestCase("patterns.Hello", "здравствуйте", true, "здравствуйте")]
    [TestCase("patterns.Hello", "пока", false)]
    [TestCase("patterns.GoodBye", "до свидания", true, "до свидания")]
    [TestCase("patterns.GoodBye", "привет", false)]
    [TestCase("patterns.GoodMorning", "с невероятно добрым утром", true, "с невероятно добрым утром")]
    [TestCase("patterns.GoodMorning", "с невероятно добрым вечером", false)]
    [TestCase("patterns.GoodMorning2", "гутен морген", true, "гутен морген")]
    [TestCase("patterns.GoodMorning2", "гутен таг", false)]
    [TestCase("patterns.GoodEvening", "с добрым вечерочком", true, "с добрым вечерочком")]
    [TestCase("patterns.GoodEvening", "с добрым утречком", false)]
    [TestCase("patterns.NumberComparison", "число пять больше чем число три", true, "число пять больше чем число три")]
    [TestCase("patterns.NumberComparison", "число пять меньше чем число три", false)]
    [TestCase("patterns.NumberComparisonWithGreeting", "привет число два", true, "привет число два")]
    [TestCase("patterns.NumberComparisonWithGreeting", "пока число два", false)]
    [TestCase("patterns.Preferable", "хотелось бы", true, "хотелось бы")]
    [TestCase("patterns.Preferable", "хотелось бык", false)]
    [TestCase("patterns.IfPossible", "по возможности", true, "по возможности")]
    [TestCase("patterns.IfPossible", "по возможностям", false)]
    [TestCase("patterns.Hours", "час", true, "час")]
    [TestCase("patterns.Hours", "чак", false)]
    [TestCase("patterns.Time", "это будет через два часа", true, "это будет через два часа")]
    [TestCase("patterns.Time", "это будет через день", false)]
    [TestCase("patterns.Time", "это было пять часов назад", true, "это было пять часов назад")]
    [TestCase("patterns.Time", "это было пять дней назад", false)]
    [TestCase("patterns.Maybe", "возможно", true, "возможно")]
    [TestCase("patterns.Maybe", "невозможно", false)]
    [TestCase("patterns.Test2", "тест тест тест", true, "тест тест тест")]
    [TestCase("patterns.Test2", "ох ах тест ах ох", true, "ох ах тест ах ох")]
    [TestCase("patterns.Test2", "ох тест ах", true, "ох тест ах")]
    [TestCase("patterns.Test2", "ох тест", true, "ох тест")]
    [TestCase("patterns.Test2", "тест ах", true, "тест ах")]
    [TestCase("patterns.Test2", "тест", true, "тест")]
    [TestCase("patterns.Test2", "бест", false)]
    [TestCase("sdn.times", "через пять часов", true, "не скоро")]
    [TestCase("sdn.times", "через один час", true, "скоро")]
    [TestCase("sdn.times", "через пять дней", false)]
    [TestCase("sdn.times", "три часа назад", true, "давно")]
    [TestCase("sdn.times", "один час назад", true, "недавно")]
    [TestCase("sdn.times", "через пять дней", false)]
    [TestCase("sdn.times.Root", "через пять часов", true, "не скоро")]
    [TestCase("sdn.times.Root", "через один час", true, "скоро")]
    [TestCase("sdn.times.Root", "через пять дней", false)]
    [TestCase("sdn.times.Root", "три часа назад", true, "давно")]
    [TestCase("sdn.times.Root", "один час назад", true, "недавно")]
    [TestCase("sdn.times.Root", "через пять дней", false)]
    [TestCase("sdn.times.Timing", "через пять часов", true, "не скоро")]
    [TestCase("sdn.times.Timing", "через один час", true, "скоро")]
    [TestCase("sdn.times.Timing", "через пять дней", false)]
    [TestCase("sdn.times.Timing", "три часа назад", true, "давно")]
    [TestCase("sdn.times.Timing", "один час назад", true, "недавно")]
    [TestCase("sdn.times.Timing", "через пять дней", false)]
    [TestCase("sdn.times.after", "через пять часов", true, "не скоро")]
    [TestCase("sdn.times.after", "через один час", true, "скоро")]
    [TestCase("sdn.times.after", "через пять дней", false)]
    [TestCase("sdn.times.before", "три часа назад", true, "давно")]
    [TestCase("sdn.times.before", "один час назад", true, "недавно")]
    [TestCase("sdn.times.before", "три дня назад", false)]
    [TestCase("sdn.digit", "пять", true, 5)]
    [TestCase("sdn.digit", "шесть", true, 6)]
    [TestCase("sdn.digit", "десять", false)]
    [TestCase("sdn.digit.Root", "пять", true, 5)]
    [TestCase("sdn.digit.Root", "шесть", true, 6)]
    [TestCase("sdn.digit.Root", "десять", false)]
    [TestCase("sdn.digit.Number", "пять", true, 5)]
    [TestCase("sdn.digit.Number", "шесть", true, 6)]
    [TestCase("sdn.digit.Number", "десять", false)]
    [TestCase("sdn.digit.Number_5", "пять", true, 5)]
    [TestCase("sdn.digit.Number_5", "шесть", false)]
    [TestCase("sdn.digits_salted_sum", "ноль плюс ноль", true, 25)]
    [TestCase("sdn.digits_salted_sum", "два плюс пять", true, 32)]
    [TestCase("sdn.digits_salted_sum", "два минус пять", false)]
    [TestCase("sdn.digits_salted_sum.Root", "ноль плюс ноль", true, 25)]
    [TestCase("sdn.digits_salted_sum.Root", "два плюс пять", true, 32)]
    [TestCase("sdn.digits_salted_sum.Root", "два минус пять", false)]
    [TestCase("sdn.digits_salted_sum.SaltedSum", "ноль плюс ноль", true, 25)]
    [TestCase("sdn.digits_salted_sum.SaltedSum", "два плюс пять", true, 32)]
    [TestCase("sdn.digits_salted_sum.SaltedSum", "два минус пять", false)]
    [TestCase("sdn.rule_space_args_test.StringValue", "ноль", true, "ноль_some fancy string_42_420")]
    [TestCase("sdn.rule_args_test.PegDefaultArgs", "пять", true, "digit_5_salt_no salt")]
    [TestCase("sdn.rule_args_test.PegDefaultRepeat", "пять", true, "digit_5_salt_eto_sol")]
    [TestCase("sdn.rule_args_test.PegDefaultSalt", "пять", true, "digit_5_salt_no saltno saltno salt")]
    [TestCase("sdn.rule_args_test.PegDefinedArgs", "пять", true, "digit_5_salt_eto_soleto_soleto_sol")]
    [TestCase("sdn.rule_args_test.RegexDefaultArgs", "пять", true, "digit_5_salt_no salt")]
    [TestCase("sdn.rule_args_test.RegexDefaultRepeat", "пять", true, "digit_5_salt_eto_sol")]
    [TestCase("sdn.rule_args_test.RegexDefaultSalt", "пять", true, "digit_5_salt_no saltno saltno salt")]
    [TestCase("sdn.rule_args_test.RegexDefinedArgs", "пять", true, "digit_5_salt_eto_soleto_soleto_sol")]
    [TestCase("sdn.multivalued_ners.Foo", "один два три", true, 3)]
    [TestCase("sdn.multivalued_ners.Foo", "один куку три", true, 1, 0)]
    [TestCase("sdn.multivalued_ners.Foo", "один два куку", true, 2, 1)]
    [TestCase("sdn.multivalued_ners.Foo", "куку два три", false)]
    [TestCase("ner.number_line", "один два три", true, 3)]
    [TestCase("ner.number_line", "один куку три", true, 1, 0)]
    [TestCase("ner.number_line", "один два куку", true, 2, 1)]
    [TestCase("ner.number_line", "куку два три", false)]
    public void MatchesAndProjects(
        string ruleKey,
        string phrase,
        bool expectedIsMatched,
        object? expectedExtractedEntity = null,
        int? expectedLastUsedSymbolIndex = null
    )
    {
        var sequence = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchResults = _ruleSpace![ruleKey].MatchAndProject(sequence, ruleSpaceArguments: _ruleSpaceArguments);

        var matchResult = matchResults.Best(_bestReferenceSelectionStrategy!);

        Assert.AreEqual(expectedIsMatched, matchResult is not null);

        if (matchResult is not null)
        {
            Assert.AreEqual(expectedLastUsedSymbolIndex ?? sequence.Length - 1, matchResult.LastUsedSymbolIndex);
            Assert.AreEqual(expectedExtractedEntity, matchResult.Result.Value);
        }
    }

    [Test]
    [TestCaseSource(nameof(FindsMatchesWithMutableArguments_Cases))]
    public void FindsMatchesWithMutableArguments(
        string ruleKey,
        string phrase,
        (Action<RuleSpaceArguments>? RuleSpaceArgumentsMutation, bool ExpectedIsMatched)[] expectedResults
    )
    {
        var sequence = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cache = new RuleSpaceCache();

        for (var index = 0; index < expectedResults.Length; index++)
        {
            var (ruleSpaceArgumentsMutation, expectedIsMatched) = expectedResults[index];

            ruleSpaceArgumentsMutation?.Invoke(_ruleSpaceArguments!);

            var isMatched = _ruleSpace![ruleKey].HasMatch(
                sequence,
                ruleSpaceArguments: _ruleSpaceArguments,
                cache: cache
            );

            Assert.AreEqual(expectedIsMatched, isMatched, $"row {index}");
        }
    }

    [Test]
    [TestCaseSource(nameof(CapturesVariables_Cases))]
    public void CapturesVariables(
        string ruleKey,
        string phrase,
        IReadOnlyDictionary<string, object?> expectedCapturedVariables
    )
    {
        var sequence = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var matchResults = _ruleSpace![ruleKey].MatchAndProject(sequence, ruleSpaceArguments: _ruleSpaceArguments);

        var matchResult = matchResults.Best(_bestReferenceSelectionStrategy!);

        Assert.IsNotNull(matchResult);

        CollectionAssert.AreEquivalent(expectedCapturedVariables, matchResult!.CapturedVariables);
    }

    [Test]
    [TestCaseSource(nameof(Prunes_Cases))]
    public void Prunes(IReadOnlySet<string> rulesToPreserve, IReadOnlySet<string> expectedRulesToBeLeft)
    {
        _ruleSpace!.Prune(rulesToPreserve);

        CollectionAssert.AreEquivalent(expectedRulesToBeLeft, _ruleSpace.Keys);
    }

    #region Sources

    #region Sources_FindsMatchesWithMutableArguments

    public static object?[][] FindsMatchesWithMutableArguments_Cases()
    {
        return new[]
        {
            new object?[]
            {
                "patterns.ReferenceToRuleWithArguments",
                "выход один",
                new (Action<RuleSpaceArguments>?, bool)[]
                {
                    (null, false),
                    (
                        arguments =>
                        {
                            var mutableParameters = (DummyMutableParameters) arguments.Values["mutable_parameters"]!;

                            mutableParameters.String = "выход";
                            mutableParameters.Int = 1;
                        },
                        true
                    ),
                    (null, true),
                    (
                        arguments =>
                        {
                            var mutableParameters = (DummyMutableParameters) arguments.Values["mutable_parameters"]!;

                            mutableParameters.String = "вход";
                            mutableParameters.Int = 2;
                        },
                        false
                    ),
                    (null, false),
                    (
                        arguments =>
                        {
                            var mutableParameters = (DummyMutableParameters) arguments.Values["mutable_parameters"]!;

                            mutableParameters.String = "выход";
                            mutableParameters.Int = 1;
                        },
                        true
                    ),
                    (null, true),
                },
            },
        };
    }

    #endregion

    #region Sources_CapturesVariables

    public static object?[][] CapturesVariables_Cases()
    {
        return new[]
        {
            new object?[]
            {
                "patterns.NumberComparisonWithGreeting",
                "привет число пять",
                new Dictionary<string, object?>
                {
                    {"greeting", "привет"},
                    {"digit", 5},
                },
            },
            new object?[]
            {
                "patterns.NumberComparisonWithGreeting",
                "добрый день число семь",
                new Dictionary<string, object?>
                {
                    {"greeting", "добрый день"},
                    {"digit", 7},
                },
            },
            new object?[]
            {
                "patterns.NumberComparisonWithGreeting",
                "я приветствую тебя число один",
                new Dictionary<string, object?>
                {
                    {"greeting", "я приветствую тебя"},
                    {"digit", 1},
                },
            },
        };
    }

    #endregion

    #region Sources_Prunes

    public static object?[][] Prunes_Cases()
    {
        return new[]
        {
            new object?[]
            {
                new HashSet<string>()
                {
                    "include2.patterns.rule1",
                    "patterns.NumberComparisonWithGreeting",
                },
                new HashSet<string>()
                {
                    "include2.patterns.rule1",
                    "include1.patterns.rule5",
                    "patterns.NumberComparisonWithGreeting",
                    "patterns.Hello",
                    "ner.digit",
                },
            },
            new object?[]
            {
                new HashSet<string>()
                {
                    "include1.patterns.rule1",
                    "include1.sdn.number1to9.Number",
                },
                new HashSet<string>()
                {
                    "include1.patterns.rule1",
                    "include1.patterns.rule2",
                    "include1.patterns.rule3",
                    "include1.sdn.number1to9.Number",
                    "include1.sdn.number1to9.Number_0",
                    "include1.sdn.number1to9.Number_1",
                    "include1.sdn.number1to9.Number_2",
                    "include1.sdn.number1to9.Number_3",
                    "include1.sdn.number1to9.Number_4",
                    "include1.sdn.number1to9.Number_5",
                    "include1.sdn.number1to9.Number_6",
                    "include1.sdn.number1to9.Number_7",
                    "include1.sdn.number1to9.Number_8",
                    "include1.sdn.number1to9.Number_9",
                },
            },
            new object?[]
            {
                new HashSet<string>()
                {
                    "include2.patterns.rule1",
                },
                new HashSet<string>()
                {
                    "include2.patterns.rule1",
                    "include1.patterns.rule5",
                },
            },
            new object?[]
            {
                new HashSet<string>()
                {
                    "patterns.ReferenceToRuleFromIncludeWithReferenceToRuleFromOtherInclude",
                },
                new HashSet<string>()
                {
                    "patterns.ReferenceToRuleFromIncludeWithReferenceToRuleFromOtherInclude",
                    "include4.patterns.rule1",
                },
            },
        };
    }

    #endregion

    #endregion
}

public sealed class DummyParameters
{
    public int Salt { get; }
    public DummySaltParameters DigitsSalt { get; }
    public DummySaltNullParameters NullDigitsSalt { get; }

    public DummyParameters(int salt, DummySaltParameters digitsSalt, DummySaltNullParameters nullDigitsSalt)
    {
        Salt = salt;
        DigitsSalt = digitsSalt;
        NullDigitsSalt = nullDigitsSalt;
    }
}

public sealed class DummySaltParameters
{
    public string Salt { get; }
    public int RepeatTimes { get; }

    public DummySaltParameters(string salt, int repeatTimes)
    {
        Salt = salt;
        RepeatTimes = repeatTimes;
    }
}

public sealed class DummySaltNullParameters
{
    public string? Salt { get; }
    public int? RepeatTimes { get; }

    public DummySaltNullParameters()
    {
        Salt = null;
        RepeatTimes = null;
    }
}

public sealed class DummyMutableParameters
{
    public int? Int { get; set; }
    public string? String { get; set; }
}